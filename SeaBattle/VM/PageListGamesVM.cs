using SeaBattle.API;
using SeaBattle.mvvm;
using SeaBattle.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SeaBattle.VM
{
    public class PageListGamesVM : BaseVM
    {
        private ObservableCollection<GameInfo> games;
        public ObservableCollection<GameInfo> Games
        {
            get => games;
            set { games = value; Signal(); }
        }

        private GameInfo selectedGame;
        public GameInfo SelectedGame
        {
            get => selectedGame;
            set { selectedGame = value; Signal(); }
        }

        public CommandVM CreateGameCommand { get; set; }
        public CommandVM RefreshCommand { get; set; }
        public CommandVM JoinGameCommand { get; set; }

        public PageListGamesVM()
        {
            Games = new ObservableCollection<GameInfo>();

            CreateGameCommand = new CommandVM(async () =>
            {
                await CreateGame();
            });

            RefreshCommand = new CommandVM(async () =>
            {
                await LoadGames();
            });

            JoinGameCommand = new CommandVM(async () =>
            {
                await JoinGame();
            });

            // Загружаем игры при создании VM
            Task.Run(LoadGames);
        }

        private async Task CreateGame()
        {
            var result = await Client.Instance.PostAsync("GameInfo/CreateGame");

            if (result.Success)
            {
                MessageBox.Show("Игра создана! Ожидаем соперника...");
                await LoadGames();
            }
            else
            {
                MessageBox.Show($"Ошибка создания игры: {result.Response}");
            }
        }

        private async Task LoadGames()
        {
            try
            {
                var result = await Client.Instance.PostAsync("GameInfo/ListGame");

                if (result.Success && !string.IsNullOrEmpty(result.Response))
                {
                    // Парсим ответ (в реальном приложении здесь был бы десериализация JSON)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Games.Clear();

                        // Заглушка для теста - добавляем тестовые игры
                        Games.Add(new GameInfo
                        {
                            Id = 1,
                            CreatorName = "Игрок1",
                            StartTime = DateTime.Now.AddMinutes(-30),
                            Status = "Ожидает игрока"
                        });

                        Games.Add(new GameInfo
                        {
                            Id = 2,
                            CreatorName = "Игрок2",
                            StartTime = DateTime.Now.AddMinutes(-15),
                            Status = "В процессе"
                        });

                        Games.Add(new GameInfo
                        {
                            Id = 3,
                            CreatorName = "Игрок3",
                            StartTime = DateTime.Now,
                            Status = "Ожидает игрока"
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки игр: {ex.Message}");
            }
        }

        private async Task JoinGame()
        {
            if (SelectedGame == null)
            {
                MessageBox.Show("Выберите игру для присоединения");
                return;
            }

            var result = await Client.Instance.PostAsync($"GameInfo/JoinGame?idGame={SelectedGame.Id}");

            if (result.Success)
            {
                MessageBox.Show($"Вы присоединились к игре {SelectedGame.CreatorName}!");
                // Здесь позже будет переход на игровую страницу
            }
            else
            {
                MessageBox.Show($"Ошибка присоединения: {result.Response}");
            }
        }
    }
}