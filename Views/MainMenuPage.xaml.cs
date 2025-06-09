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
    }
}
