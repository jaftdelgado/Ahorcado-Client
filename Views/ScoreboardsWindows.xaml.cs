using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class ScoreboardsWindows : UserControl
    {
        private List<PlayerMatchHistory> _matchHistory = new List<PlayerMatchHistory>();

        public ScoreboardsWindows()
        {
            InitializeComponent();
            Loaded += ScoreboardsWindows_Loaded;
        }

        private async void ScoreboardsWindows_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadMatchHistoryAsync();
        }

        private async Task LoadMatchHistoryAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                int playerId = CurrentSession.LoggedInPlayer.PlayerID;

                var dtoList = client.GetPlayerMatchHistory(playerId);

                var localModels = dtoList.Select(dto => new PlayerMatchHistory
                {
                    MatchID = dto.MatchID,
                    OpponentName = dto.OpponentName,
                    PlayedWord = dto.PlayedWord,
                    ResultID = dto.ResultID,
                    Points = dto.Points,
                    EndDate = dto.EndDate
                }).OrderByDescending(p => p.EndDate).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _matchHistory = localModels;
                    DgMatches.ItemsSource = _matchHistory;
                });
            });
        }

        private void DgMatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Opcional: mostrar detalles si se desea
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public static void Show()
        {
            PopUpUtilities.ShowDialog(new ScoreboardsWindows());
        }

        public void Close()
        {
            PopUpUtilities.CloseDialog();
        }
    }
}
