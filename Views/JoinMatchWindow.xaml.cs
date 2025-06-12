using AhorcadoClient.CallbackServiceReference;
using AhorcadoClient.CallbackServices;
using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class JoinMatchWindow : UserControl
    {
        private List<Match> _availableMatches = new List<Match>();

        public JoinMatchWindow()
        {
            InitializeComponent();
            Loaded += JoinMatchWindow_Loaded;
        }

        private async void JoinMatchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAvailableMatches();
        }

        private async Task LoadAvailableMatches()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var matchDTOs = client.GetAvailableMatches();

                var matches = matchDTOs.Select(dto => new Match
                {
                    MatchID = dto.MatchID,
                    Player1 = dto.Player1ID,
                    Player1Username = dto.Player1.Username,
                    Player2 = dto.Player2ID,
                    WordID = dto.WordID,
                    CreateDate = dto.CreateDate,
                    EndDate = dto.EndDate,
                    StatusID = dto.StatusID,
                    Word = dto.Word == null ? null : new Word
                    {
                        WordID = dto.Word.WordID,
                        CategoryID = dto.Word.CategoryID,
                        LanguageID = dto.Word.LanguageID,
                        WordText = dto.Word.Word,
                        Description = dto.Word.Description,
                        Difficult = dto.Word.Difficult
                    }
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _availableMatches = matches;
                    DgMatches.ItemsSource = _availableMatches;
                    UpdateButtonState();
                });
            });
        }

        private async Task JoinMatch()
        {
            var selectedMatch = DgMatches.SelectedItem as Match;
            if (selectedMatch == null) return;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var currentPlayer = CurrentSession.LoggedInPlayer;

                // Obtener el DTO de jugador actual
                var playerDTO = new PlayerInfoDTO
                {
                    PlayerId = currentPlayer.PlayerID,
                    Username = currentPlayer.Username,
                    FullName = $"{currentPlayer.FirstName} {currentPlayer.LastName}",
                    ProfilePic = currentPlayer.ProfilePic
                };

                // Obtener el WordInfoDTO desde matchDTO.Word
                // Asumiendo que client.JoinMatch devuelve el MatchInfoDTO actualizado
                var matchDTO = client.JoinMatch(selectedMatch.MatchID, playerDTO.PlayerId);

                if (matchDTO == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                        MessageDialog.Show("Error", "No se pudo unir a la partida", AlertType.WARNING, () => Close())
                    );
                    return;
                }

                var callback = new GameCallbackClient();
                var gameService = new GameServiceClient(callback);

                var wordDTO = matchDTO.Word != null ? new WordInfoDTO
                {
                    WordID = matchDTO.Word.WordID,
                    CategoryID = matchDTO.Word.CategoryID,
                    LanguageID = matchDTO.Word.LanguageID,
                    WordText = matchDTO.Word.Word,
                    Description = matchDTO.Word.Description,
                    Difficult = matchDTO.Word.Difficult
                } : null;

                gameService.JoinMatch(matchDTO.MatchID, playerDTO, wordDTO);
                var matchInfoDTO = new MatchInfoDTO
                {
                    MatchID = matchDTO.MatchID,
                    Player1 = matchDTO.Player1 != null ? new PlayerInfoDTO
                    {
                        PlayerId = matchDTO.Player1.PlayerID,
                        Username = matchDTO.Player1.Username,
                        FullName = $"{matchDTO.Player1.FirstName}",
                        ProfilePic = matchDTO.Player1.ProfilePic
                    } : null,
                    Player2 = playerDTO,
                    Word = wordDTO
                };

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Close();
                    NavigationManager.Instance.NavigateToPage(new MatchPage(matchInfoDTO, gameService));
                });
            });
        }


        public static void Show()
        {
            PopUpUtilities.ShowDialog(new JoinMatchWindow());
        }

        public void Close()
        {
            PopUpUtilities.CloseDialog();
        }

        private void UpdateButtonState()
        {
            BtnJoinGame.IsEnabled = DgMatches.SelectedItem != null;
        }

        private async void Click_BtnJoinGame(object sender, RoutedEventArgs e)
        {
            await JoinMatch();
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgMatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtonState();
        }


    }
}
