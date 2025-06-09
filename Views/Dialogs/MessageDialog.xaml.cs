using AhorcadoClient.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AhorcadoClient.Views.Dialogs
{
    public partial class MessageDialog : UserControl
    {
        private Action _onConfirm;
        public static Action OnCancel;
        public static readonly (string Brush, string ButtonText) DangerStyle = ("DangerBrush", "Glb_Delete");

        public MessageDialog()
        {
            InitializeComponent();
        }

        public static void Show(string title, string description, AlertType alertType, Action onConfirm = null)
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            var dialog = new MessageDialog
            {
                _onConfirm = onConfirm
            };

            dialog.Opacity = 0;
            dialog.DialogTitle.Text = FindResourceString(title);
            dialog.DialogDescription.Text = FindResourceString(description);
            dialog.ConfigureAlertType(alertType);

            Action showAction = () =>
            {
                var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                dialogHost.Content = dialog;
                dialogOverlay.Visibility = Visibility.Visible;
                Animations.BeginAnimation(dialog, "PopupFadeInAnimation");
            };

            if (Application.Current.Dispatcher.CheckAccess())
                showAction();
            else
                Application.Current.Dispatcher.Invoke(showAction);
        }

        public static void ShowConfirm(string title, string description, Action onConfirm,
            string dangerButtonTextResourceKey = null)
        {
            var activeWindow = GetActiveWindow();
            if (activeWindow == null) return;

            var dialog = new MessageDialog
            {
                _onConfirm = onConfirm
            };

            dialog.DialogTitle.Text = FindResourceString(title);
            dialog.DialogDescription.Text = FindResourceString(description);
            dialog.SetAlertProperties("Glb_Accept", Visibility.Visible);
            dialog.BtnClose.Visibility = Visibility.Visible;

            if (string.IsNullOrEmpty(dangerButtonTextResourceKey))
                dialog.BtnAccept.Content = FindResourceString("Glb_Confirm");
            else
            {
                dialog.BtnAccept.Background = (SolidColorBrush)Application.Current.Resources["DangerBrush"];
                dialog.BtnAccept.Content = FindResourceString(dangerButtonTextResourceKey);
            }

            Action showAction = () =>
            {
                var dialogHost = activeWindow.FindName("DialogHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("DialogOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                dialogHost.Content = dialog;
                dialogOverlay.Visibility = Visibility.Visible;
                Animations.BeginAnimation(dialog, "PopupFadeInAnimation");
            };

            if (Application.Current.Dispatcher.CheckAccess())
                showAction();
            else
                Application.Current.Dispatcher.Invoke(showAction);
        }

        private static string FindResourceString(string resourceKey)
        {
            var resource = Application.Current.TryFindResource(resourceKey);
            return resource != null ? resource.ToString() : $"{resourceKey}";
        }

        private void ConfigureAlertType(AlertType alertType)
        {
            string buttonTextResourceKey;

            switch (alertType)
            {
                case AlertType.SUCCESS:
                    buttonTextResourceKey = "Glb_Accept";
                    break;
                case AlertType.WARNING:
                    buttonTextResourceKey = "Glb_TryAgain";
                    break;
                case AlertType.ERROR:
                    buttonTextResourceKey = "Glb_Close";
                    break;
                default:
                    buttonTextResourceKey = "Glb_Confirm";
                    break;
            }

            SetAlertProperties(buttonTextResourceKey, Visibility.Collapsed);
        }

        private void SetAlertProperties(string buttonTextResourceKey, Visibility secondaryButtonVisibility)
        {
            BtnAccept.Content = FindResourceString(buttonTextResourceKey);
            BtnClose.Visibility = secondaryButtonVisibility;
        }

        private void Click_BtnAccept(object sender, RoutedEventArgs e)
        {
            _onConfirm?.Invoke();
            CloseDialog();
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            OnCancel?.Invoke();
            OnCancel = null;
            CloseDialog();
        }

        private void CloseDialog()
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

        private static Window GetActiveWindow()
        {
            return Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.IsActive);
        }
    }

    public enum AlertType
    {
        SUCCESS,
        WARNING,
        ERROR
    }
}
