using AhorcadoClient.Model;
using AhorcadoClient.ServiceReference;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace AhorcadoClient.Views
{
    public partial class SignInWindow : Window
    {
        public SignInWindow()
        {
            InitializeComponent();
            UpdateFormButtonState();

            var languages = Model.Language.GetDefaultLanguages();
            CbPreferedLanguage.ItemsSource = languages;
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
            var password = PbLoginPassword.Password;

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
                    Password = password,
                    SelectedLanguageID = dto.SelectedLanguageID
                };

                CurrentSession.SetUser(player);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                    MessageDialog.Show("SignIn_DialogTSignedIn", "SignIn_DialogDSignedIn", AlertType.SUCCESS, () => NavigateToMain())
                );
            });
        }

        private void UpdateFormButtonState()
        {
            var requiredFields = new List<TextBox>
            {
                TbFirstName, TbLastName,
                TbPhoneNumber, TbEmailAddress,
                TbUsername, TbPassword,
                TbBirthDay
            };

            bool allFieldsFilled = true;

            foreach (var field in requiredFields)
            {
                if (string.IsNullOrWhiteSpace(field.Text))
                {
                    allFieldsFilled = false;
                    break;
                }
            }

            BtnCreateAccount.IsEnabled = allFieldsFilled;
        }

        private async void Click_BtnSignIn(object sender, RoutedEventArgs e)
        {
            await Login();
        }

        private async void Click_BtnCreateAccount(object sender, RoutedEventArgs e)
        {
            await RegisterAccount();
            
        }

        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SelectProfileImage(PlayerProfilePic, "GlbDialogT_SelectProfilePic", () =>
            {
                BtnDeleteImage.IsEnabled = true;
            });
        }

        private void Click_BtnRegister(object sender, MouseButtonEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            CreateAccountPanel.Visibility = Visibility.Visible;
        }

        private void Click_BtnDeleteImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SetImageSource(PlayerProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
            BtnDeleteImage.IsEnabled = false;
        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            CreateAccountPanel.Visibility = Visibility.Collapsed;
            SignInPanel.Visibility = Visibility.Visible;
        }

        private void RequiredFields_TextChanged(object sender, RoutedEventArgs e)
        {
            UpdateFormButtonState();
            ValidateForm();
        }
        
        private void Password_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && PbPassword.Password != textBox.Text)
                PbPassword.Password = textBox.Text;
            else if (sender is PasswordBox passwordBox && TbPassword.Text != passwordBox.Password)
                TbPassword.Text = passwordBox.Password;

            UpdateFormButtonState();
            ValidateForm();
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.ShowPassword(TbPassword, PbPassword);
            UpdateFormButtonState();
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.HidePassword(TbPassword, PbPassword);
            UpdateFormButtonState();
        }

        private async Task RegisterAccount()
        {
            if (!DateTime.TryParse(TbBirthDay.Text.Trim(), out DateTime birthDay))
            {
                MessageDialog.Show("SignIn_DialogTInvalidDate", "SignIn_DialogDInvalidDate", AlertType.ERROR);
                return;
            }

            var player = new PlayerDTO
            {
                FirstName = TbFirstName.Text.Trim(),
                LastName = TbLastName.Text.Trim(),
                BirthDay = birthDay,
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                EmailAddress = TbEmailAddress.Text.Trim(),
                Username = TbUserName.Text.Trim(),
                Password = TbPassword.Text.Trim(),
                ProfilePic = ImageUtilities.ImageToByteArray(PlayerProfilePic.Source as BitmapSource),
                SelectedLanguageID = (CbPreferedLanguage.SelectedItem as LanguageDTO)?.LanguageID ?? 1
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.RegisterPlayer(player);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (result)
                    {
                        MessageDialog.Show("SignIn_DialogTAccountCreated", "SignIn_DialogDAccountCreated", AlertType.SUCCESS, () =>
                        {
                            CreateAccountPanel.Visibility = Visibility.Collapsed;
                            SignInPanel.Visibility = Visibility.Visible;
                        });
                    }
                    else
                    {
                        MessageDialog.Show("SignIn_DialogTUserExists", "SignIn_DialogDUserExists", AlertType.ERROR);
                    }
                });
            });
    
        }
        
        private void ValidateForm()
        {
            // Verifica si todos los campos obligatorios tienen contenido
            bool isFormValid =
                !string.IsNullOrWhiteSpace(TbFirstName.Text) &&
                !string.IsNullOrWhiteSpace(TbLastName.Text) &&
                !string.IsNullOrWhiteSpace(TbPhoneNumber.Text) &&
                !string.IsNullOrWhiteSpace(TbEmailAddress.Text) &&
                !string.IsNullOrWhiteSpace(TbBirthDay.Text) &&
                CbPreferedLanguage.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(PbPassword.Password); // o TbPassword.Text si estás mostrando la contraseña

            BtnCreateAccount.IsEnabled = isFormValid;
        }

    }
}
