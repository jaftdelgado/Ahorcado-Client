using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Utilities
{
    public static class PopUpUtilities
    {
        public static void ShowDialog(UserControl content)
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            content.Opacity = 0;

            Action showAction = () =>
            {
                var dialogHost = activeWindow.FindName("PopUpHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("PopUpOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                dialogHost.Content = content;
                dialogOverlay.Visibility = Visibility.Visible;

                Animations.BeginAnimation(content, "PopupFadeInAnimation");
            };

            if (Application.Current.Dispatcher.CheckAccess())
                showAction();
            else
                Application.Current.Dispatcher.Invoke(showAction);
        }

        public static void CloseDialog()
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

        private static Window GetActiveWindow()
        {
            return Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        }
    }
}
