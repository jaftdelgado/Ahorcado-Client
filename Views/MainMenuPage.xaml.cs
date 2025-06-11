using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
        }

        private void Click_BtnCreateGame(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateGameWindow.Show();
        }

        private void Click_BtnJoinMatch(object sender, System.Windows.RoutedEventArgs e)
        {
            JoinMatchPage.Show();
        }
    }
}
