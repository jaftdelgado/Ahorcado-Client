using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class CreateGameWindow : UserControl
    {
        public CreateGameWindow()
        {
            InitializeComponent();
            Loaded += CreateGameWindow_Loaded;
        }

        private async void CreateGameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarCategoriasAsync();
        }

        private async Task CargarCategoriasAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var categorias = client.GetCategories(); 
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CbCategory.ItemsSource = categorias;
                    CbWord.DisplayMemberPath = "Name";
                    CbWord.SelectedValuePath = "CategoryID";
                    CbDifficult.IsEnabled = false;
                    CbWord.IsEnabled = false;
                });
            });
        }

        private async void CbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbCategory.SelectedItem == null) return;
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var dificultades = client.GetDifficults((int)CbCategory.SelectedItem);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CbDifficult.ItemsSource = dificultades;
                    CbDifficult.IsEnabled = true;
                    CbWord.IsEnabled = false;
                    CbWord.ItemsSource = null;
                });
            });
        }

        private async void CbDifficult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbDifficult.SelectedItem == null || CbCategory.SelectedItem == null) return;
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var palabras = client.GetWords(
                            ((Category)CbCategory.SelectedItem).CategoryID,
                            (int)CbDifficult.SelectedItem
                );
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CbWord.ItemsSource = palabras;
                    CbWord.DisplayMemberPath = "word";
                    CbWord.SelectedValuePath = "WordID";
                    CbWord.IsEnabled = true;
                });
            });
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
                var dialogHost = activeWindow.FindName("PopUpHost") as ContentControl;
                var dialogOverlay = activeWindow.FindName("PopUpOverlay") as Border;

                if (dialogHost == null || dialogOverlay == null) return;

                dialogHost.Content = createGameWindow;
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

        private async void Click_BtnCreateGameAsync(object sender, RoutedEventArgs e)
        {
            if (CbCategory.SelectedItem == null)
            {
                MessageDialog.Show("Error", "PorFavorSeleccionaCategoria", AlertType.ERROR);
                return;
            }
            if (CbDifficult.SelectedItem == null)
            {
                MessageDialog.Show("Error", "PorFavorSeleccionaDificultad", AlertType.ERROR);
                return;
            }
            if (CbWord.SelectedItem == null)
            {
                MessageDialog.Show("Error", "PorFavorSeleccionaPalabra", AlertType.ERROR);
                return;
            }

            int playerId = CurrentSession.LoggedInPlayer.PlayerID;
            int palabraId = (int)CbWord.SelectedValue;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                client.CreateMatch(playerId, palabraId);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("Glb_CreateGame", "Partida creada correctamente.", AlertType.SUCCESS, Close);
                });
            });

            //Enviar a la ventana de juego
        }
    }
}
