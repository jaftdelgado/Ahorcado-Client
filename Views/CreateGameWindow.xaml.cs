using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class CreateGameWindow : UserControl
    {
        public CreateGameWindow()
        {
            InitializeComponent();
        }

        public static void Show()
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            var createGameWindow = new CreateGameWindow
            {
                Opacity = 0
            };

            Action showAction = () =>
            {
                var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                dialogHost.Content = createGameWindow;
                dialogOverlay.Visibility = Visibility.Visible;

                Utilities.Animations.BeginAnimation(createGameWindow, "PopupFadeInAnimation");
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
                var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

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
    }
}
