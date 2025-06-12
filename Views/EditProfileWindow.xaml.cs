using AhorcadoClient.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class EditProfileWindow : UserControl
    {
        public EditProfileWindow()
        {
            InitializeComponent();
        }

        public static void Show()
        {
            PopUpUtilities.ShowDialog(new EditProfileWindow());
        }

        public void Close()
        {
            PopUpUtilities.CloseDialog();
        }


        private void Click_BtnSelectImage(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnDeleteImage(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnEditAccount(object sender, RoutedEventArgs e)
        {

        }

        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RequiredFields_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Password_TextChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

        }
    }
}
