using System.Linq;
using System.Windows;
using AhorcadoClient.Utilities;

namespace AhorcadoClient.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationManager.Initialize(MainFrame);

            var navigationManager = NavigationManager.Instance;
            NavigateToMainMenu();

        }
        private void NavigateToMainMenu()
        {
            var menuPage = new MainMenuPage();
            menuPage.SignOutRequested += OnSignOutRequested;

            MainFrame.Navigate(menuPage);
        }

        private void OnSignOutRequested()
        {
            var loginWindow = new SignInWindow();
            loginWindow.Show();
            
            
            Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault()?
                .Close();
        }
    }
}