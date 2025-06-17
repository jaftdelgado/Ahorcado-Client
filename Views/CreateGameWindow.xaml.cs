using AhorcadoClient.Utilities;
using AhorcadoClient.Model;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using AhorcadoClient.CallbackServices;
using AhorcadoClient.CallbackServiceReference;
using System;

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
            int palabraId = ((Word)CbWord.SelectedItem).WordID;

            await ServiceClientManager.ExecuteServerAction(async () =>
            {
                var client = ServiceClientManager.Instance.Client;
                var matchDTO = client.CreateMatch(playerId, palabraId);

                var player = CurrentSession.LoggedInPlayer;

                var playerDTO = new PlayerInfoDTO
                {
                    PlayerId = player.PlayerID,
                    Username = player.Username,
                    FullName = $"{player.FirstName} {player.LastName}",
                    ProfilePic = player.ProfilePic
                };

                var matchInfoDTO = new MatchInfoDTO
                {
                    MatchID = matchDTO.MatchID,
                    Player1 = playerDTO,
                    Player2 = null,
                    Word = new WordInfoDTO
                    {
                        WordID = matchDTO.Word.WordID,
                        CategoryID = matchDTO.Word.CategoryID,
                        LanguageID = matchDTO.Word.LanguageID,
                        WordText = matchDTO.Word.Word,
                        Description = matchDTO.Word.Description,
                        Difficult = matchDTO.Word.Difficult
                    }
                };

                var gameCallback = new GameCallbackClient();

                var gameService = new GameServiceClient(gameCallback);
                gameService.JoinMatch(matchDTO.MatchID, playerDTO, matchInfoDTO.Word, 6);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Close();
                    NavigationManager.Instance.NavigateToPage(new MatchPage(matchInfoDTO, gameService));
                });
            });
        }
    }
}
