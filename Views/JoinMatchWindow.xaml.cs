using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
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
                var matchDTOs = client.GetAvailableMatches(CurrentSession.LoggedInPlayer.PlayerID);

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

        private void Click_BtnJoinGame(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DgMatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
