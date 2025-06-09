using AhorcadoClient.Model;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System.Threading.Tasks;
using System.Windows;

namespace AhorcadoClient.Views
{
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
        }

        private void NavigateToMain()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private async Task Login()
        {
            var user = TbUsername.Text;
            var password = PbPassword.Password;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                var dto = client.LogIn(user, password);

                if (dto == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        MessageDialog.Show("SignIn_DialogTInvalidCreds", "SignIn_DialogDInvalidCreds", AlertType.ERROR);
                    });
                    return;
                }

                var player = new Player
                {
                    PlayerID = dto.PlayerID,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    BirthDay = dto.BirthDay,
                    PhoneNumber = dto.PhoneNumber,
                    EmailAddress = dto.EmailAddress,
                    ProfilePic = dto.ProfilePic,
                    TotalScore = dto.TotalScore,
                    Username = dto.Username,
                    SelectedLanguageID = dto.SelectedLanguageID
                };

                CurrentSession.SetUser(player);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageDialog.Show("SignIn_DialogTSignedIn", "SignIn_DialogDSignedIn", AlertType.SUCCESS, () => NavigateToMain())
                );
            });
        }

        private async void Click_BtnSignIn(object sender, RoutedEventArgs e)
        {
            await Login();
        }
    }
}
