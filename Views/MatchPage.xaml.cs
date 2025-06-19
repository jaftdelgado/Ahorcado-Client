using AhorcadoClient.CallbackServiceReference;
using AhorcadoClient.CallbackServices;
using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class MatchPage : Page
    {
        private string _word;
        private MatchInfoDTO _matchInfo;
        private List<TextBox> _letterBoxes = new List<TextBox>();
        private int _remainingAttempts = 6;
        private GameServiceClient _gameService;
        private HashSet<string> _guessedLetters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool IsPlayer1 => _matchInfo.Player1?.PlayerId == CurrentSession.LoggedInPlayer.PlayerID;
        private bool IsPlayer2 => _matchInfo.Player2?.PlayerId == CurrentSession.LoggedInPlayer.PlayerID;

        public string Word
        {
            get => _word;
            set
            {
                _word = value.ToUpper();
                GenerateLetterBoxes();
            }
        }

        public MatchPage(MatchInfoDTO matchInfo, GameServiceClient gameService)
        {
            InitializeComponent();

            _matchInfo = matchInfo;
            _remainingAttempts = 6;
            _gameService = gameService;

            _gameService.EnsureConnection();

            if (matchInfo?.Word != null)
            {
                Word = matchInfo.Word.WordText;
            }

            SetPlayersInfo();
            AttachCallbacks();
            ConfigureUIByRole(matchInfo);
            WordHint.Text = _matchInfo.Word.Description;
            UpdateAttemptsText();
            //UpdateHangmanImage();
            SetWordDescription();
        }

        private void SetWordDescription()
        {
            if (_matchInfo?.Word == null) return;

            var categories = Category.GetDefaultCategories();
            var languages = Model.Language.GetDefaultLanguages();

            var category = categories.Find(c => c.CategoryID == _matchInfo.Word.CategoryID);
            var language = languages.Find(l => l.LanguageID == _matchInfo.Word.LanguageID);

            WordCategory.Text = category?.CategoryName ?? "N/A";
            WordLanguage.Text = language?.LanguageName ?? "N/A";
        }

        private async Task DeclareMatchResult(MatchInfoDTO match)
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                int currentPlayerID = CurrentSession.LoggedInPlayer.PlayerID;
                bool resultDeclared = false;
                bool isWinner = false;
                string opponentUsername = null;

                if (match.Player1.PlayerId == currentPlayerID)
                {
                    if (_remainingAttempts <= 0)
                    {
                        resultDeclared = client.DeclareVictoryForPlayer1(match.MatchID);
                        isWinner = resultDeclared;
                    }
                    opponentUsername = match.Player2?.Username;
                }
                else if (match.Player2?.PlayerId == currentPlayerID)
                {
                    if (_remainingAttempts > 0)
                    {
                        resultDeclared = client.DeclareVictoryForPlayer2(match.MatchID);
                        isWinner = resultDeclared;
                    }
                    opponentUsername = match.Player1.Username;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isWinner) ShowVictoryDialog(opponentUsername);

                    else ShowDefeatDialog(opponentUsername);
                });
            });
        }

        private void ShowVictoryDialog(string opponentUsername)
        {
            string title, message;

            if (IsPlayer1)
            {
                title = (string)Application.Current.Resources["Match_DialogTVictoryP1"];
                message = string.Format((string)Application.Current.Resources["Match_DialogDVictoryP1"], opponentUsername);
            }
            else
            {
                title = (string)Application.Current.Resources["Match_DialogTVictoryP2"];
                message = string.Format((string)Application.Current.Resources["Match_DialogDVictoryP2"], opponentUsername);
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationManager.Instance.NavigateToPage(new MainMenuPage());
        }

        private void ShowDefeatDialog(string opponentUsername)
        {
            string title, message;

            if (IsPlayer1)
            {
                title = (string)Application.Current.Resources["Match_DialogTDefeatP1"];
                message = string.Format((string)Application.Current.Resources["Match_DialogDDefeatP1"], opponentUsername);
            }
            else
            {
                title = (string)Application.Current.Resources["Match_DialogTDefeatP2"];
                message = string.Format((string)Application.Current.Resources["Match_DialogDDefeatP2"], opponentUsername);
            }

            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationManager.Instance.NavigateToPage(new MainMenuPage());
        }

        private void SetPlayersInfo()
        {
            if (_matchInfo.Player1 != null)
            {
                Player1Username.Text = _matchInfo.Player1.Username ?? (string)Application.Current.FindResource("Match_DefaultPlayer1Name");
                Player1FullName.Text = _matchInfo.Player1.FullName ?? string.Empty;
                ImageUtilities.SetImageSource(Player1Pic, _matchInfo.Player1.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);
            }

            if (_matchInfo.Player2 != null)
            {
                Player2Username.Text = _matchInfo.Player2.Username ?? (string)Application.Current.FindResource("Match_DefaultPlayer2Name");
                Player2FullName.Text = _matchInfo.Player2.FullName ?? string.Empty;
                ImageUtilities.SetImageSource(Player2Pic, _matchInfo.Player2.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);
            }
            else
            {
                Player2Username.Text = (string)Application.Current.FindResource("Match_WaitingForPlayer");
                Player2FullName.Text = string.Empty;
            }
        }

        private void ConfigureUIByRole(MatchInfoDTO matchInfo)
        {
            if (IsPlayer1)
            {
                WordSelected.Visibility = Visibility.Visible;
                WordText.Text = matchInfo.Word.WordText;
                WordDescription.Visibility = Visibility.Collapsed;
                KeyboardPanel.IsEnabled = false;

                Word = matchInfo.Word.WordText;

                if (matchInfo.GuessedLetters != null)
                {
                    _guessedLetters = new HashSet<string>(matchInfo.GuessedLetters, StringComparer.OrdinalIgnoreCase);
                    UpdateGuessedLetters(_guessedLetters);
                }
                else
                {
                    _guessedLetters.Clear();
                }
            }
            else if (IsPlayer2)
            {
                WordSelected.Visibility = Visibility.Collapsed;
                WordDescription.Visibility = Visibility.Visible;
                KeyboardPanel.IsEnabled = true;
            }
        }

        private void UpdatePlayerInfo(TextBlock usernameText, TextBlock fullNameText, Image profileImage, PlayerInfoDTO playerInfo)
        {
            usernameText.Text = playerInfo.Username ?? "Jugador";
            fullNameText.Text = playerInfo.FullName ?? "";
            ImageUtilities.SetImageSource(profileImage, playerInfo.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);
        }

        private void AttachCallbacks()
        {
            if (_gameService.Callback != null)
            {
                _gameService.Callback.OnMatchReadyAction = (matchId, matchInfo) => OnMatchReady(matchInfo);
                _gameService.Callback.OnPlayerJoinedAction = (matchId, playerInfo) => OnPlayerJoined(playerInfo);
                _gameService.Callback.OnPlayerLeftAction = (matchId, playerId) => OnPlayerLeft(playerId);
                _gameService.Callback.OnLetterGuessedAction = (matchId, letter, isCorrect, remainingAttempts, isGameOver) =>
                    OnLetterGuessed(letter, isCorrect, remainingAttempts, isGameOver);
                _gameService.Callback.OnGameOverAction = (matchId, winnerPlayerId) => OnGameOver(winnerPlayerId);
            }
        }

        private void OnMatchReady(MatchInfoDTO matchInfo)
        {
            Dispatcher.Invoke(() =>
            {
                _matchInfo = matchInfo;
                SetPlayersInfo();

                UpdateGameState(matchInfo.RemainingAttempts, matchInfo.GuessedLetters);

                ShowMatchReadyNotification();
            });
        }

        private void OnPlayerJoined(PlayerInfoDTO playerInfo)
        {
            Dispatcher.Invoke(() =>
            {
                if (_matchInfo.Player1?.PlayerId == playerInfo.PlayerId)
                {
                    UpdatePlayerInfo(Player1Username, Player1FullName, Player1Pic, playerInfo);
                }
                else if (_matchInfo.Player2?.PlayerId == playerInfo.PlayerId)
                {
                    UpdatePlayerInfo(Player2Username, Player2FullName, Player2Pic, playerInfo);

                    if (IsPlayer2) KeyboardPanel.IsEnabled = true;
                }
            });
        }

        private async void OnPlayerLeft(int playerId)
        {
            await Dispatcher.Invoke(async () =>
            {
                bool isCurrentPlayer = playerId == CurrentSession.LoggedInPlayer.PlayerID;
                bool isOpponentLeft = !isCurrentPlayer;

                KeyboardPanel.IsEnabled = false;
                DisableAllKeyButtons();
                RevealFullWord();

                if (isOpponentLeft)
                {
                    string opponentUsername = IsPlayer1
                        ? _matchInfo.Player2?.Username ?? (string)Application.Current.FindResource("Match_DefaultOpponentName")
                        : _matchInfo.Player1?.Username ?? (string)Application.Current.FindResource("Match_DefaultOpponentName");

                    string message = string.Format(
                        (string)Application.Current.FindResource("Match_OpponentLeftMessage"),
                        opponentUsername);

                    MessageBox.Show(
                        message,
                        (string)Application.Current.FindResource("Match_GameEndedTitle"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    DetachCallbacks();


                    NavigationManager.Instance.NavigateToPage(new MainMenuPage());
                }
                else
                {
                    NavigationManager.Instance.NavigateToPage(new MainMenuPage());
                }
            });
        }

        private void OnLetterGuessed(string letter, bool isCorrect, int remainingAttempts, bool isGameOver)
        {
            Dispatcher.Invoke(() =>
            {
                UpdateGameState(remainingAttempts, new List<string> { letter });

                ShowLetterGuessNotification(letter, isCorrect);

                if (isGameOver)
                {
                    HandleGameOver(isCorrect);
                    KeyboardPanel.IsEnabled = false;
                }
                else
                {
                    if (IsPlayer2 && _remainingAttempts > 0) KeyboardPanel.IsEnabled = true;
                }
            });
        }

        private void OnGameOver(int winnerPlayerId)
        {
            Dispatcher.Invoke(() =>
            {
                RevealFullWord();
                bool isWinner = winnerPlayerId == CurrentSession.LoggedInPlayer.PlayerID;
                string opponentUsername = IsPlayer1 ? _matchInfo.Player2.Username : _matchInfo.Player1.Username;

                if (isWinner)
                    ShowVictoryDialog(opponentUsername);
                else
                    ShowDefeatDialog(opponentUsername);

                KeyboardPanel.IsEnabled = false;
                DisableAllKeyButtons();
                DetachCallbacks();

            });
        }

        private void UpdateGameState(int remainingAttempts, IEnumerable<string> guessedLetters)
        {
            _remainingAttempts = remainingAttempts;
            UpdateAttemptsText();
            UpdateHangmanImage();

            if (guessedLetters != null)
            {
                foreach (var letter in guessedLetters)
                {
                    _guessedLetters.Add(letter);
                }

                UpdateGuessedLetters(_guessedLetters);
            }
        }

        private void UpdateGuessedLetters(IEnumerable<string> guessedLetters)
        {
            if (guessedLetters == null || _letterBoxes.Count == 0) return;

            int boxIndex = 0;
            for (int i = 0; i < Word.Length; i++)
            {
                if (Word[i] == ' ') continue;

                string letter = Word[i].ToString();

                if (guessedLetters.Any(l => l.Equals(letter, StringComparison.OrdinalIgnoreCase)))
                    _letterBoxes[boxIndex].Text = letter.ToUpper();

                else
                    _letterBoxes[boxIndex].Text = "";

                boxIndex++;
            }
        }

        private void ShowLetterGuessNotification(string letter, bool isCorrect)
        {
            if (IsPlayer1)
            {
                string message = isCorrect
                    ? $"El Jugador 2 acertó la letra '{letter}'"
                    : $"El Jugador 2 intentó la letra '{letter}' (incorrecta)";

                MessageDialog.Show("Turno del oponente", message,
                    isCorrect ? AlertType.SUCCESS : AlertType.WARNING);
            }
            else if (IsPlayer2 && !isCorrect)
            {

                MessageBox.Show("Letra incorrecta",
                    $"La letra '{letter}' no está en la palabra");
            }
        }

        private async void HandleGameOver(bool isWordGuessed)
        {
            DisableAllKeyButtons();
            RevealFullWord();

            if (_remainingAttempts <= 0)
            {
                if (IsPlayer1) await DeclareMatchResult(_matchInfo);
            }
            else if (isWordGuessed)
            {
                if (IsPlayer2) await DeclareMatchResult(_matchInfo);
            }
        }

        private void ShowMatchReadyNotification()
        {
            if (_matchInfo.Player2?.PlayerId == CurrentSession.LoggedInPlayer.PlayerID)
            {
                MessageBox.Show(
                    (string)Application.Current.FindResource("Match_DialogMatchReadyP2Message"),
                    (string)Application.Current.FindResource("Match_DialogMatchReadyP2Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else if (IsPlayer1)
            {
                MessageBox.Show(
                    (string)Application.Current.FindResource("Match_DialogMatchReadyP1Message"),
                    (string)Application.Current.FindResource("Match_DialogMatchReadyP1Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private void RevealFullWord()
        {
            int boxIndex = 0;
            for (int i = 0; i < Word.Length; i++)
            {
                if (Word[i] == ' ') continue;
                _letterBoxes[boxIndex].Text = Word[i].ToString();
                boxIndex++;
            }
        }

        private void GenerateLetterBoxes()
        {
            LetterPanel.Children.Clear();
            _letterBoxes.Clear();

            if (string.IsNullOrEmpty(Word)) return;

            foreach (char c in Word)
            {
                if (c == ' ')
                {
                    var spacer = new Border { Width = 20 };
                    LetterPanel.Children.Add(spacer);
                    continue;
                }

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("LetterTextBoxStyle"),
                    Margin = new Thickness(3, 0, 3, 0),
                    IsReadOnly = true,
                    Text = ""
                };

                _letterBoxes.Add(textBox);
                LetterPanel.Children.Add(textBox);
            }
        }

        private async void Click_KeyButton(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !IsPlayer2) return;

            string guessedLetter = button.Content.ToString();
            button.IsEnabled = false;
            KeyboardPanel.IsEnabled = false;

            try
            {
                bool result = await Task.Run(() =>
                {
                    _gameService.GuessLetter(_matchInfo.MatchID, CurrentSession.LoggedInPlayer.PlayerID, guessedLetter);
                    return true;
                });
            }
            catch (Exception ex)
            {
                MessageDialog.Show("Error", "No se pudo enviar la letra: " + ex.Message, AlertType.ERROR);
                button.IsEnabled = true;
                KeyboardPanel.IsEnabled = true;
            }
        }

        private void UpdateAttemptsText()
        {
            AttemptsText.Text = $"{_remainingAttempts}/6";
        }

        private void UpdateHangmanImage()
        {
            string imagePath = $"/Resources/Images/ahorcado-{_remainingAttempts}.png";
            HangmanImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(imagePath, UriKind.Relative));
        }

        private void DisableAllKeyButtons()
        {
            foreach (var child in KeyboardPanel.Children)
            {
                if (child is Panel panel)
                {
                    foreach (var innerChild in panel.Children)
                    {
                        if (innerChild is Button button) button.IsEnabled = false;
                    }
                }
            }
        }

        private void Click_BtnLeaveMatch(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                (string)Application.Current.FindResource("Match_LeaveConfirmationMessage"),
                (string)Application.Current.FindResource("Match_LeaveConfirmationTitle"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _gameService.LeaveMatch(_matchInfo.MatchID, CurrentSession.LoggedInPlayer.PlayerID);
                DetachCallbacks();
                NavigationManager.Instance.NavigateToPage(new MainMenuPage());
            }
        }
        public void DetachCallbacks()
        {
            if (_gameService?.Callback != null)
            {
                _gameService.Callback.OnMatchReadyAction = null;
                _gameService.Callback.OnPlayerJoinedAction = null;
                _gameService.Callback.OnPlayerLeftAction = null;
                _gameService.Callback.OnLetterGuessedAction = null;
                _gameService.Callback.OnGameOverAction = null;
            }
        }

    }
}