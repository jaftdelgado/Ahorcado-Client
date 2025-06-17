using AhorcadoClient.CallbackServiceReference;
using AhorcadoClient.CallbackServices;
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
            UpdateAttemptsText();
            UpdateHangmanImage();
        }

        private async Task DeclareMatchResultAsync(MatchInfoDTO match)
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
                    // Jugador 1 solo declara victoria cuando Jugador 2 se queda sin intentos
                    if (_remainingAttempts <= 0) // Jugador 2 perdió
                    {
                        resultDeclared = client.DeclareVictoryForPlayer1(match.MatchID);
                        isWinner = resultDeclared;
                    }
                    opponentUsername = match.Player2?.Username;
                }
                else if (match.Player2?.PlayerId == currentPlayerID)
                {
                    // Jugador 2 solo declara victoria cuando adivina la palabra
                    if (_remainingAttempts > 0) // Aún quedan intentos (adivinó la palabra)
                    {
                        resultDeclared = client.DeclareVictoryForPlayer2(match.MatchID);
                        isWinner = resultDeclared;
                    }
                    opponentUsername = match.Player1.Username;
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (isWinner)
                    {
                        ShowVictoryDialog(opponentUsername);
                    }
                    else
                    {
                        ShowDefeatDialog(opponentUsername);
                    }
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
                Player1Username.Text = _matchInfo.Player1.Username ?? "Jugador 1";
                Player1FullName.Text = _matchInfo.Player1.FullName ?? "";
                ImageUtilities.SetImageSource(Player1Pic, _matchInfo.Player1.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);
            }

            if (_matchInfo.Player2 != null)
            {
                Player2Username.Text = _matchInfo.Player2.Username ?? "Jugador 2";
                Player2FullName.Text = _matchInfo.Player2.FullName ?? "";
                ImageUtilities.SetImageSource(
                    Player2Pic,
                    _matchInfo.Player2.ProfilePic,
                    Constants.DEFAULT_PROFILE_PIC_PATH
                );
            }
            else
            {
                Player2Username.Text = "Esperando jugador...";
                Player2FullName.Text = "";
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

        private void OnPlayerLeft(int playerId)
        {
            Dispatcher.Invoke(() =>
            {
                if (_matchInfo.Player1?.PlayerId == playerId)
                {
                    _matchInfo.Player1 = null;
                    Player1Username.Text = "Jugador desconectado";
                    Player1FullName.Text = "";
                }
                else if (_matchInfo.Player2?.PlayerId == playerId)
                {
                    _matchInfo.Player2 = null;
                    Player2Username.Text = "Esperando jugador...";
                    Player2FullName.Text = "";
                    KeyboardPanel.IsEnabled = false;
                }

                MessageDialog.Show("Jugador abandonó", "El otro jugador ha abandonado la partida", AlertType.WARNING);
            });
        }

        private void OnLetterGuessed(string letter, bool isCorrect, int remainingAttempts, bool isGameOver)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine($"Jugador {CurrentSession.LoggedInPlayer.PlayerID} recibió letra: {letter}");

                UpdateGameState(remainingAttempts, new List<string> { letter });

                ShowLetterGuessNotification(letter, isCorrect);

                if (isGameOver)
                {
                    HandleGameOver(isCorrect);
                    KeyboardPanel.IsEnabled = false;
                }
                else
                {
                    if (IsPlayer2 && _remainingAttempts > 0)
                    {
                        KeyboardPanel.IsEnabled = true;
                    }
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
                {
                    _letterBoxes[boxIndex].Text = letter.ToUpper();
                }
                else
                {
                    _letterBoxes[boxIndex].Text = ""; 
                }

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
                
                MessageDialog.Show("Letra incorrecta",
                    $"La letra '{letter}' no está en la palabra",
                    AlertType.WARNING);
            }
        }

        private async void HandleGameOver(bool isWordGuessed)
        {
            DisableAllKeyButtons();
            RevealFullWord();

            if (_remainingAttempts <= 0)
            {
                if (IsPlayer1) await DeclareMatchResultAsync(_matchInfo);
            }
            else if (isWordGuessed)
            {
                if (IsPlayer2) await DeclareMatchResultAsync(_matchInfo);
            }
        }

        private void ShowMatchReadyNotification()
        {
            if (_matchInfo.Player2?.PlayerId == CurrentSession.LoggedInPlayer.PlayerID)
            {
                MessageDialog.Show("Partida lista", "¡Te has unido a la partida!", AlertType.SUCCESS);
            }
            else if (IsPlayer1)
            {
                MessageDialog.Show("Jugador conectado", "Un jugador se ha unido a tu partida", AlertType.SUCCESS);
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
    }
}