using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class MainMenuPage : Page
    {
        public Action SignOutRequested { get; internal set; }

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

        private void NavigateToSignIn()
        {
            var currentWindow = Application.Current.Windows
                .OfType<MainWindow>()
                .FirstOrDefault();
            var signInWindow = new SignInWindow();
            signInWindow.Show();
            currentWindow?.Close();
        }

        private void Click_BtnCreateGame(object sender, System.Windows.RoutedEventArgs e)
        {
            CreateGameWindow.Show();
        }

        private void Click_BtnJoinMatch(object sender, System.Windows.RoutedEventArgs e)
        {
            JoinMatchWindow.Show();
        }

        private void Click_BtnViewScores(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewScoresPage.Show();
        }

        private void Click_BtnProfile(object sender, System.Windows.RoutedEventArgs e)
        {
            EditProfileWindow.Show(CurrentSession.LoggedInPlayer);
        }


        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureCode = selectedItem.Tag.ToString();
                ((App)Application.Current).ChangeCulture(cultureCode);
            }
        }

        private void Click_BtnSignOut(object sender, RoutedEventArgs e)
        {
            MessageDialog.ShowConfirm(
                "SignOut_DialogTSignOut", "SignOut_DialogDSignOut",
                () =>
                {
                    NavigationManager.Instance.CurrentPageAs<MatchPage>()?.DetachCallbacks();
                    CurrentSession.LogOut();
                    NavigateToSignIn();
                }
            );
        }
    }
}