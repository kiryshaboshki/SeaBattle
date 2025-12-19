using SeaBattle.API;
using SeaBattle.mvvm;
using SeaBattle.Models;
using SeaBattle.View;
using System;
using System.Collections.ObjectModel;
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

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            set { isLoading = value; Signal(); }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set { statusMessage = value; Signal(); }
        }

        public CommandVM CreateGameCommand { get; set; }
        public CommandVM RefreshCommand { get; set; }
        public CommandVM JoinGameCommand { get; set; }
        public CommandVM ViewProfileCommand { get; set; }

        public PageListGamesVM()
        {
            Games = new ObservableCollection<GameInfo>();
            IsLoading = false;
            StatusMessage = "Загрузка списка игр...";

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

            ViewProfileCommand = new CommandVM(() =>
            {
                MessageBox.Show("Профиль пользователя (в разработке)", "Профиль");
            });

            // Загружаем игры при создании VM
            Task.Run(LoadGames);
        }

        private async Task CreateGame()
        {
            IsLoading = true;
            StatusMessage = "Создание новой игры...";

            try
            {
                var result = await Client.Instance.PostAsync("GameInfo/CreateGame");

                if (result.Success)
                {
                    // Десериализуем ответ (в реальном приложении)
                    // var game = JsonSerializer.Deserialize<GameInfo>(result.Response);

                    MessageBox.Show("Игра успешно создана! Переходим к расстановке кораблей.",
                        "Игра создана", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переход на страницу расстановки кораблей
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new ShipPlacementPage();
                }
                else
                {
                    MessageBox.Show($"Ошибка создания игры: {result.Response}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение при создании игры: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                StatusMessage = "Готово";
            }
        }

        private async Task LoadGames()
        {
            IsLoading = true;
            StatusMessage = "Загрузка списка игр...";

            try
            {
                var result = await Client.Instance.PostAsync("GameInfo/ListGame");

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Games.Clear();

                    if (result.Success && !string.IsNullOrEmpty(result.Response))
                    {
                        // В реальном приложении здесь десериализация JSON
                        // Games = JsonSerializer.Deserialize<ObservableCollection<GameInfo>>(result.Response);

                        // Заглушка для теста
                        AddTestGames();
                        StatusMessage = $"Загружено {Games.Count} игр";
                    }
                    else
                    {
                        // Если API недоступно, показываем тестовые данные
                        AddTestGames();
                        StatusMessage = $"Используются тестовые данные ({Games.Count} игр)";
                    }
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddTestGames();
                    StatusMessage = $"Ошибка загрузки: {ex.Message}. Тестовые данные";
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddTestGames()
        {
            Games.Add(new GameInfo
            {
                Id = 1,
                CreatorName = "Администратор",
                StartTime = DateTime.Now.AddMinutes(-45),
                Status = "Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2
            });

            Games.Add(new GameInfo
            {
                Id = 2,
                CreatorName = "Игрок_Pro",
                StartTime = DateTime.Now.AddMinutes(-20),
                Status = "В процессе",
                PlayersCount = 2,
                MaxPlayers = 2
            });

            Games.Add(new GameInfo
            {
                Id = 3,
                CreatorName = "Новичок",
                StartTime = DateTime.Now.AddMinutes(-5),
                Status = "Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2
            });

            Games.Add(new GameInfo
            {
                Id = 4,
                CreatorName = "Моряк",
                StartTime = DateTime.Now.AddMinutes(-60),
                Status = "Завершена",
                PlayersCount = 2,
                MaxPlayers = 2,
                Winner = "Моряк"
            });

            Games.Add(new GameInfo
            {
                Id = 5,
                CreatorName = "Капитан",
                StartTime = DateTime.Now,
                Status = "Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2
            });
        }

        private async Task JoinGame()
        {
            if (SelectedGame == null)
            {
                MessageBox.Show("Выберите игру из списка для присоединения",
                    "Выбор игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedGame.Status != "Ожидает игрока")
            {
                MessageBox.Show("Эта игра уже началась или завершена. Выберите другую игру.",
                    "Игра недоступна", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            StatusMessage = $"Присоединение к игре {SelectedGame.CreatorName}...";

            try
            {
                var result = await Client.Instance.PostAsync($"GameInfo/JoinGame?idGame={SelectedGame.Id}");

                if (result.Success)
                {
                    MessageBox.Show($"Вы успешно присоединились к игре {SelectedGame.CreatorName}!\nПереходим к расстановке кораблей.",
                        "Присоединение успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переход на страницу расстановки кораблей
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new ShipPlacementPage();
                }
                else
                {
                    MessageBox.Show($"Ошибка присоединения: {result.Response}\nВозможно, игра уже началась.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Исключение при присоединении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                StatusMessage = "Готово";
            }
        }
    }
}