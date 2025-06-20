using AhorcadoClient.Model;
using AhorcadoClient.ServiceReference;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Collections.Generic;
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
            App.ApplyCurrentCulture();
            InitializeComponent();
            UpdateFormButtonState();

            var languages = Model.Language.GetDefaultLanguages();
            CbPreferedLanguage.ItemsSource = languages;
            SetSelectedLanguage();
        }

        private void NavigateToMain()
        {
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void SetSelectedLanguage()
        {
            foreach (ComboBoxItem item in cbLanguages.Items)
            {
                if (item.Tag?.ToString() == App.CurrentCultureCode)
                {
                    cbLanguages.SelectedItem = item;
                    break;
                }
            }
        }


        private async Task Login()
        {
            var user = TbUsername.Text;
            var password = PbLogInPassword.Password;

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                MessageDialog.Show("SignIn_DialogTInvalidCreds", "SignIn_DialogVInvalidCreds", AlertType.ERROR);
                return;
            }

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

        private void PasswordLogIn_TextChanged(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && PbLogInPassword.Password != textBox.Text)
                PbLogInPassword.Password = textBox.Text;
            else if (sender is PasswordBox passwordBox && TbLogInPassword.Text != passwordBox.Password)
                TbLogInPassword.Text = passwordBox.Password;
        }

        private void ShowPasswordLogInCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.ShowPassword(TbLogInPassword, PbLogInPassword);
            UpdateFormButtonState();
        }

        private void ShowPasswordLogInCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordUtilities.HidePassword(TbLogInPassword, PbLogInPassword);
            UpdateFormButtonState();
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

        private bool ValidateFields()
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(TbFirstName.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDNameLettersOnly", AlertType.ERROR);
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(TbLastName.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDLastNameLettersOnly", AlertType.ERROR);
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(TbPhoneNumber.Text, @"^\d{10}$"))
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDPhoneInvalid", AlertType.ERROR);
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(TbEmailAddress.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDEmailInvalid", AlertType.ERROR);
                return false;
            }

            if (!DateTime.TryParse(TbBirthDay.Text, out DateTime birthDay) || birthDay > DateTime.Now)
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDBirthDateInvalid", AlertType.ERROR);
                return false;
            }

            if (TbUserName.Text.Length < 4)
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDUserShort", AlertType.ERROR);
                return false;
            }
            if (TbPassword.Text.Length < 8)
            {
                MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDPasswordShort", AlertType.ERROR);
                return false;
            }

            string[] peligrosos = { "'", "\"", ";", "--", "/*", "*/" };
            foreach (var campo in new[] { TbFirstName.Text, TbLastName.Text, TbUserName.Text, TbPassword.Text })
            {
                foreach (var p in peligrosos)
                {
                    if (campo.Contains(p))
                    {
                        MessageDialog.Show("SignIn_DialogTValidation", "SignIn_DialogDSpecialChars", AlertType.ERROR);
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task RegisterAccount()
        {
            if (!ValidateFields())
                return;

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
            bool isFormValid =
                !string.IsNullOrWhiteSpace(TbFirstName.Text) &&
                !string.IsNullOrWhiteSpace(TbLastName.Text) &&
                !string.IsNullOrWhiteSpace(TbPhoneNumber.Text) &&
                !string.IsNullOrWhiteSpace(TbEmailAddress.Text) &&
                !string.IsNullOrWhiteSpace(TbBirthDay.Text) &&
                CbPreferedLanguage.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(PbPassword.Password);

            BtnCreateAccount.IsEnabled = isFormValid;
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbLanguages.SelectedItem is ComboBoxItem selectedItem)
            {
                string cultureCode = selectedItem.Tag.ToString();

                App.CurrentCultureCode = cultureCode;
                ((App)Application.Current).ChangeCulture(cultureCode);
                App.ApplyCurrentCulture();
            }
        }

    }
}
