using AhorcadoClient.ServiceReference;
using AhorcadoClient.Utilities;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class MainMenuPage : Page
    {
        public MainMenuPage()
        {
            InitializeComponent();
            ConfigureProfileButton();
        }

        private void ConfigureProfileButton()
        {
            var player = CurrentSession.LoggedInPlayer;
            if (player == null) return;

            BtnProfile.ApplyTemplate();

            var profileImage = BtnProfile.Template.FindName("PART_ProfileImage", BtnProfile) as Image;
            var nameBlock = BtnProfile.Template.FindName("PART_UserName", BtnProfile) as TextBlock;
            var scoreBlock = BtnProfile.Template.FindName("PART_PlayerPoints", BtnProfile) as TextBlock;

            ImageUtilities.SetImageSource(profileImage, player.ProfilePic, Constants.DEFAULT_PROFILE_PIC_PATH);

            if (nameBlock != null) nameBlock.Text = player.Username;
            if (scoreBlock != null) scoreBlock.Text = player.DisplayScore;
        }

        private void Click_BtnCreateGame(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateGameWindow.Show();
        }

        private void Click_BtnJoinMatch(object sender, System.Windows.RoutedEventArgs e)
        {
            JoinMatchPage.Show();
        }

        private void Click_BtnProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            EditProfileWindow.Show(CurrentSession.LoggedInPlayer);
        }
    }
}