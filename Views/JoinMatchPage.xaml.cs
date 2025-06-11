using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AhorcadoClient.Views
{
    public partial class JoinMatchPage : Page
    {
        public JoinMatchPage()
        {
            InitializeComponent();
            Loaded += JoinMatchPage_Loaded;
        }

        private async void JoinMatchPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPartidasDisponiblesAsync();
        }

        private async System.Threading.Tasks.Task CargarPartidasDisponiblesAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var partidas = client.GetAvailableMatches();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    DgMatches.ItemsSource = partidas;
                });
            });
        }

        public static void Show()
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            var createGameWindow = new JoinMatchPage
            {
                Opacity = 0
            };

            Action showAction = () =>
            {
                var dialogHost = activeWindow.FindName("PopUpHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("PopUpOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                var frame = new Frame
                {
                    Width = 800,
                    Height = 450,
                    Margin = new Thickness(40),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };
                frame.Content = new JoinMatchPage();
                dialogHost.Content = frame;
                dialogOverlay.Visibility = Visibility.Visible;

                Animations.BeginAnimation(createGameWindow, "PopupFadeInAnimation");
            };

            if (Application.Current.Dispatcher.CheckAccess())
                showAction();
            else
                Application.Current.Dispatcher.Invoke(showAction);
        }

        private static Window GetActiveWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }

        public void Close()
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow != null)
            {
                var dialogHost = activeWindow.FindName("PopUpHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("PopUpOverlay") as Border;

                if (dialogHost != null && dialogOverlay != null)
                {
                    dialogHost.Content = null;
                    dialogOverlay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void Click_BtnJoinMatch(object sender, RoutedEventArgs e)
        {
            if (DgMatches.SelectedItem == null)
            {
                MessageDialog.Show("Error", "Por favor selecciona una partida para unirte.", AlertType.ERROR);
                return;
            }

            dynamic selectedMatch = DgMatches.SelectedItem;
            int matchId = selectedMatch.MatchID;
            int playerId = CurrentSession.LoggedInPlayer.PlayerID;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                client.JoinMatch(matchId, playerId);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("Unirse a partida", "Te has unido correctamente a la partida.", AlertType.SUCCESS, Close);
                });
            });

            //Enviar a la página de juego
        }
    }
}
