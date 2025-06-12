using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AhorcadoClient.Views
{
    public partial class ViewScoresPage : Page
    {
        public ViewScoresPage()
        {
            InitializeComponent();
            Loaded += ViewScoresPage_Loaded;
        }

        private async void ViewScoresPage_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPartidasAsync();
        }

        private async System.Threading.Tasks.Task CargarPartidasAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var partidas = client.GetPlayerMatchHistory(CurrentSession.LoggedInPlayer.PlayerID);
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

            var createGameWindow = new ViewScoresPage
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
                frame.Content = new ViewScoresPage();
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

        private void Click_BtnReturn(object sender, RoutedEventArgs e)
        {
            Close();
        }

    }
}
