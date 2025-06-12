using AhorcadoClient.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace AhorcadoClient.Views
{
    public partial class MatchPage : Page
    {
        private string _word;
        private List<TextBox> _letterBoxes = new List<TextBox>();
        private int _remainingAttempts = 6;

        public string Word
        {
            get => _word;
            set
            {
                _word = value.ToUpper();
                GenerateLetterBoxes();
            }
        }

        public MatchPage(Match match)
        {
            InitializeComponent();

            if (match?.Word != null)
                Word = match.Word.WordText;

            UpdateAttemptsText();
        }

        private void GenerateLetterBoxes()
        {
            LetterPanel.Children.Clear();
            _letterBoxes.Clear();

            foreach (char c in Word)
            {
                if (c == ' ')
                {
                    var spacer = new Border { Width = 20 };
                    LetterPanel.Children.Add(spacer);
                    continue;
                }

                var textBox = new TextBox
                {
                    Style = (Style)FindResource("LetterTextBoxStyle"),
                    Margin = new Thickness(3, 0, 3, 0),
                    IsReadOnly = true,
                    Text = ""
                };

                _letterBoxes.Add(textBox);
                LetterPanel.Children.Add(textBox);
            }
        }

        private void Click_KeyButton(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string guessedLetter = button.Content.ToString().ToUpper();
                button.IsEnabled = false;

                bool matchFound = false;
                int boxIndex = 0;

                for (int i = 0; i < Word.Length; i++)
                {
                    if (Word[i] == ' ')
                        continue;

                    if (Word[i].ToString() == guessedLetter)
                    {
                        _letterBoxes[boxIndex].Text = guessedLetter;
                        matchFound = true;
                    }

                    boxIndex++;
                }

                if (!matchFound)
                {
                    _remainingAttempts--;
                    UpdateAttemptsText();
                    UpdateHangmanImage();

                    if (_remainingAttempts == 0)
                    {
                        MessageBox.Show("Has perdido. No te quedan más intentos.", "Fin del juego", MessageBoxButton.OK, MessageBoxImage.Error);
                        DisableAllKeyButtons();
                    }
                    else
                    {
                        MessageBox.Show($"La letra '{guessedLetter}' no está en la palabra.", "Letra incorrecta", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    if (IsWordGuessed())
                    {
                        MessageBox.Show("¡Felicidades! Has adivinado la palabra.", "Victoria", MessageBoxButton.OK, MessageBoxImage.Information);
                        DisableAllKeyButtons();
                    }
                }
            }
        }

        private void UpdateAttemptsText()
        {
            AttemptsText.Text = $"{_remainingAttempts}/6";
        }

        private void UpdateHangmanImage()
        {
            string imagePath = $"/Resources/Images/ahorcado-{_remainingAttempts}.png";
            HangmanImage.Source = new System.Windows.Media.Imaging.BitmapImage(new System.Uri(imagePath, System.UriKind.Relative));
        }

        private void DisableAllKeyButtons()
        {
            foreach (var child in KeyboardPanel.Children)
            {
                if (child is Panel panel)
                {
                    foreach (var innerChild in panel.Children)
                    {
                        if (innerChild is Button button) button.IsEnabled = false;
                    }
                }
            }
        }

        private bool IsWordGuessed()
        {
            foreach (var box in _letterBoxes)
            {
                if (string.IsNullOrWhiteSpace(box.Text))
                    return false;
            }
            return true;
        }
    }
}