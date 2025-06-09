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
            navigationManager.NavigateToPage(new MainMenuPage());
        }

    }
}