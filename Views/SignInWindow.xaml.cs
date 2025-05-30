using AhorcadoClient.Views.Dialogs;
using System;
using System.Collections.Generic;
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

        private void Click_BtnSignIn(object sender, RoutedEventArgs e)
        {
            MessageDialog.Show("PRUEBA", "prueba de meesage", AlertType.SUCCESS);
        }
    }
}
