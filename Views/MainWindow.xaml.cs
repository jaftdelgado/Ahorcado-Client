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

            App.ApplyCurrentCulture(); 

            NavigationManager.Initialize(MainFrame);
            Application.Current.MainWindow = this;
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var navigationManager = NavigationManager.Instance;
            NavigateToMainMenu();

        }

        private void NavigateToMainMenu()
        {
            var menuPage = new MainMenuPage();
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
            CurrentSession.LogOut();
        }
    }
}