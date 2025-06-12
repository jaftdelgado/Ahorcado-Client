using AhorcadoClient.Utilities;
using AhorcadoClient.Model;
using AhorcadoClient.Views.Dialogs;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace AhorcadoClient.Views
{
    public partial class CreateGameWindow : UserControl
    {
        private int playerId = CurrentSession.LoggedInPlayer.PlayerID;

        private List<Word> _allWords;

        public CreateGameWindow()
        {
            InitializeComponent();
            Loaded += CreateGameWindow_Loaded;

            var categorias = Category.GetDefaultCategories();
            CbCategory.ItemsSource = categorias;

            var languages = Model.Language.GetDefaultLanguages();
            CbLanguage.ItemsSource = languages;

            var difficulties = Difficulty.GetDefaultDifficulties();
            CbDifficulty.ItemsSource = difficulties;

            UpdateFormButtonState();
        }

        private async void CreateGameWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarPalabrasAsync();
        }

        private async Task CargarPalabrasAsync()
        {
            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var wordDTOs = client.GetWords();

                var palabras = wordDTOs.Select(dto => new Word
                {
                    WordID = dto.WordID,
                    CategoryID = dto.CategoryID,
                    LanguageID = dto.LanguageID,
                    WordText = dto.Word,
                    Description = dto.Description,
                    Difficult = dto.Difficult
                }).ToList();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    _allWords = palabras;
                    CbWord.IsEnabled = false;
                });
            });
        }

        public static void Show()
        {
            PopUpUtilities.ShowDialog(new CreateGameWindow());
        }

        public void Close()
        {
            PopUpUtilities.CloseDialog();
        }

        private void UpdateFormButtonState()
        {
            var requiredSelections = new List<ComboBox>
            {
                CbCategory,
                CbLanguage,
                CbDifficulty,
                CbWord
            };

            bool allSelected = true;

            foreach (var comboBox in requiredSelections)
            {
                if (comboBox.SelectedItem == null)
                {
                    allSelected = false;
                    break;
                }
            }

            BtnCreateGame.IsEnabled = allSelected;
        }


        private void Click_BtnCancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void FiltrarYMostrarPalabras()
        {
            if (CbLanguage.SelectedItem == null || CbCategory.SelectedItem == null || CbDifficulty.SelectedItem == null)
            {
                CbWord.IsEnabled = false;
                CbWord.ItemsSource = null;
                return;
            }

            var categoria = (Category) CbCategory.SelectedItem;
            var idioma = (Language) CbLanguage.SelectedItem;
            var dificultad = (Difficulty) CbDifficulty.SelectedItem;

            var filtradas = _allWords.Where(p =>
                p.CategoryID == categoria.CategoryID &&
                p.LanguageID == idioma.LanguageID &&
                p.Difficult == dificultad.DifficultyID).ToList();

            CbWord.ItemsSource = filtradas;
            CbWord.IsEnabled = filtradas.Count > 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarYMostrarPalabras();
            UpdateFormButtonState();
        }

        private void CbWord_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFormButtonState();
        }

        private async void Click_BtnCreateGame(object sender, RoutedEventArgs e)
        {
            int palabraId = (int)CbWord.SelectedValue;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                client.CreateMatch(playerId, palabraId);
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    MessageDialog.Show("Glb_CreateGame", "Partida creada correctamente.", AlertType.SUCCESS, Close);
                });
            });

        }
    }
}
