using AhorcadoClient.Model;
using AhorcadoClient.ServiceReference;
using AhorcadoClient.Utilities;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AhorcadoClient.Views
{
    public partial class EditProfileWindow : UserControl
    {
        private readonly Player _player;

        public EditProfileWindow(Player player)
        {
            InitializeComponent();
            _player = player;
            Loaded += SignInWindow_Loaded;
            UpdateFormButtonState();
        }

        public static void Show(Player player)
        {
            var window = new EditProfileWindow(player);
            window.LoadPlayerData(player);
            PopUpUtilities.ShowDialog(window);
        }

        public void Close()
        {
            PopUpUtilities.CloseDialog();
        }


        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SelectProfileImage(PlayerProfilePic, "GlbDialogT_SelectProfilePic", () =>
            {
                BtnDeleteImage.IsEnabled = true;
            });
        }

        private void Click_BtnDeleteImage(object sender, RoutedEventArgs e)
        {
            ImageUtilities.SetImageSource(PlayerProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
            BtnDeleteImage.IsEnabled = false;
        }

        private bool ValidarCampos()
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(TbFirstName.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageDialog.Show("Validación", "El nombre solo debe contener letras y espacios.", AlertType.ERROR);
                return false;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(TbLastName.Text, @"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$"))
            {
                MessageDialog.Show("Validación", "El apellido solo debe contener letras y espacios.", AlertType.ERROR);
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(TbPhoneNumber.Text, @"^\d{8,}$"))
            {
                MessageDialog.Show("Validación", "El teléfono debe contener solo números y al menos 8 dígitos.", AlertType.ERROR);
                return false;
            }

            if (!DateTime.TryParse(TbBirthDay.Text, out DateTime birthDay) || birthDay > DateTime.Now)
            {
                MessageDialog.Show("Validación", "Introduce una fecha de nacimiento válida.", AlertType.ERROR);
                return false;
            }

            if (TbUserName.Text.Length < 4)
            {
                MessageDialog.Show("Validación", "El usuario debe tener al menos 4 caracteres.", AlertType.ERROR);
                return false;
            }
            if (TbPassword.Text.Length < 4)
            {
                MessageDialog.Show("Validación", "La contraseña debe tener al menos 4 caracteres.", AlertType.ERROR);
                return false;
            }

            string[] peligrosos = { "'", "\"", ";", "--", "/*", "*/" };
            foreach (var campo in new[] { TbFirstName.Text, TbLastName.Text, TbUserName.Text, TbPassword.Text })
            {
                foreach (var p in peligrosos)
                {
                    if (campo.Contains(p))
                    {
                        MessageDialog.Show("Validación", "No se permiten caracteres especiales como comillas, punto y coma o guiones.", AlertType.ERROR);
                        return false;
                    }
                }
            }

            return true;
        }

        private async void Click_BtnEditAccount(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            if (!DateTime.TryParse(TbBirthDay.Text.Trim(), out DateTime birthDay))
            {
                MessageDialog.Show("EditProfile_DialogTInvalidDate", "EditProfile_DialogDInvalidDate", AlertType.ERROR);
                return;
            }
            var currentPlayer = CurrentSession.LoggedInPlayer;

            var player = new PlayerDTO
            {
                PlayerID = currentPlayer.PlayerID,
                FirstName = TbFirstName.Text.Trim(),
                LastName = TbLastName.Text.Trim(),
                BirthDay = birthDay,
                PhoneNumber = TbPhoneNumber.Text.Trim(),
                Username = TbUserName.Text.Trim(),
                Password = TbPassword.Text.Trim(),
                ProfilePic = ImageUtilities.ImageToByteArray(PlayerProfilePic.Source as BitmapSource),
                SelectedLanguageID = (CbPreferedLanguage.SelectedItem as LanguageDTO)?.LanguageID ?? 1
            };

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                if (client == null) return;

                bool result = client.UpdatePlayerInfo(player); // Usa tu método para actualizar al jugador

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (result)
                    {
                        MessageDialog.Show("EditProfile_DialogTChangesSaved", "EditProfile_DialogDChangesSaved", AlertType.SUCCESS, () =>
                        {
                            Close(); // Cierra el formulario
                        });
                    }
                    else
                    {
                        MessageDialog.Show("EditProfile_DialogTUpdateFailed", "EditProfile_DialogDUpdateFailed", AlertType.ERROR);
                    }
                });
            });
        }


        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RequiredFields_TextChanged(object sender, TextChangedEventArgs e)
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
        private void UpdateFormButtonState()
        {
            var requiredFields = new List<TextBox>
            {
                TbFirstName, TbLastName,
                TbPhoneNumber,
                TbUserName, TbPassword,
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

            BtnEditAccount.IsEnabled = allFieldsFilled;
        }
        private void ValidateForm()
        {
            // Verifica si todos los campos obligatorios tienen contenido
            bool isFormValid =
                !string.IsNullOrWhiteSpace(TbFirstName.Text) &&
                !string.IsNullOrWhiteSpace(TbLastName.Text) &&
                !string.IsNullOrWhiteSpace(TbPhoneNumber.Text) &&
                !string.IsNullOrWhiteSpace(TbBirthDay.Text) &&
                CbPreferedLanguage.SelectedItem != null &&
                !string.IsNullOrWhiteSpace(PbPassword.Password); // o TbPassword.Text si estás mostrando la contraseña

            BtnEditAccount.IsEnabled = isFormValid;
        }
        public void LoadPlayerData(Player player)
        {
            // Cargar datos de texto
            TbFirstName.Text = player.FirstName;
            TbLastName.Text = player.LastName;
            TbPhoneNumber.Text = player.PhoneNumber;
            TbBirthDay.Text = player.BirthDay.ToString("yyyy-MM-dd");
            TbUserName.Text = player.Username;
                // Ajusta formato si es necesario

            // Cargar contraseña
            TbPassword.Text = player.Password;
            PbPassword.Password = player.Password;

            // Cargar imagen de perfil (si hay)
            if (player.ProfilePic != null)
            {
                PlayerProfilePic.Source = ImageUtilities.ByteArrayToImage(player.ProfilePic);
                BtnDeleteImage.IsEnabled = true;
            }
            else
            {
                ImageUtilities.SetImageSource(PlayerProfilePic, null, Constants.DEFAULT_PROFILE_PIC_PATH);
                BtnDeleteImage.IsEnabled = false;
            }

            // Seleccionar idioma en el ComboBox
            if (CbPreferedLanguage.ItemsSource is IEnumerable<LanguageDTO> languages)
            {
                foreach (var language in languages)
                {
                    if (language.LanguageID == player.SelectedLanguageID)
                    {
                        CbPreferedLanguage.SelectedItem = language;
                        break;
                    }
                }
            }

            UpdateFormButtonState();
            ValidateForm();
        }
        private async Task CargarIdiomasAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var idiomas = client.GetLanguages();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    CbPreferedLanguage.ItemsSource = idiomas;
                    CbPreferedLanguage.DisplayMemberPath = "LanguageName";
                });
            });
        }
        private async void SignInWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await CargarIdiomasAsync();
            LoadPlayerData(_player);
        }
    }
}
