using SeaBattle.API;
using SeaBattle.mvvm;
using SeaBattle.Models;
using SeaBattle.View;
using System;
using System.Collections.ObjectModel;
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
        public CommandVM ExitCommand { get; set; }

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
                MessageBox.Show("Профиль пользователя\n(функция в разработке)",
                    "Профиль", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            ExitCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Завершить работу приложения?",
                    "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
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
                    // В реальном приложении здесь десериализация ответа
                    // try 
                    // {
                    //     var game = JsonSerializer.Deserialize<GameInfo>(result.Response);
                    //     MessageBox.Show($"Игра #{game.Id} создана!", "Успех");
                    // }
                    // catch { }

                    MessageBox.Show("🎮 Игра успешно создана!\n\nТеперь переходим к расстановке кораблей.",
                        "Игра создана", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переход на страницу расстановки кораблей
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new ShipPlacementPage();
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка создания игры:\n{result.Response}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠ Исключение при создании игры:\n{ex.Message}",
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

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Games.Clear();

                    if (result.Success && !string.IsNullOrEmpty(result.Response))
                    {
                        try
                        {
                            // Пытаемся десериализовать реальный ответ
                            // var gamesList = JsonSerializer.Deserialize<List<GameInfo>>(result.Response);
                            // foreach (var game in gamesList)
                            // {
                            //     Games.Add(game);
                            // }
                            // StatusMessage = $"Загружено {Games.Count} активных игр";

                            // Если API недоступно или формат неверный, используем тестовые данные
                            AddTestGames();
                            StatusMessage = $"📋 Загружено {Games.Count} игр (тестовые данные)";
                        }
                        catch
                        {
                            AddTestGames();
                            StatusMessage = "📋 Используются тестовые данные";
                        }
                    }
                    else
                    {
                        // Если API недоступно, показываем тестовые данные
                        AddTestGames();
                        StatusMessage = $"📋 {Games.Count} игр (API недоступен)";
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AddTestGames();
                    StatusMessage = $"⚠ Ошибка: {ex.Message.Split('\n')[0]}";
                });
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddTestGames()
        {
            Games.Clear();

            Games.Add(new GameInfo
            {
                Id = 101,
                CreatorName = "Администратор",
                StartTime = DateTime.Now.AddMinutes(-45),
                Status = "👤 Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2,
                CreatorRating = 1850
            });

            Games.Add(new GameInfo
            {
                Id = 102,
                CreatorName = "Игрок_Pro",
                StartTime = DateTime.Now.AddMinutes(-20),
                Status = "⚔ В процессе",
                PlayersCount = 2,
                MaxPlayers = 2,
                CreatorRating = 2100
            });

            Games.Add(new GameInfo
            {
                Id = 103,
                CreatorName = "Новичок",
                StartTime = DateTime.Now.AddMinutes(-5),
                Status = "👤 Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2,
                CreatorRating = 1200
            });

            Games.Add(new GameInfo
            {
                Id = 104,
                CreatorName = "Моряк",
                StartTime = DateTime.Now.AddMinutes(-60),
                Status = "🏁 Завершена",
                PlayersCount = 2,
                MaxPlayers = 2,
                Winner = "Моряк",
                CreatorRating = 1950
            });

            Games.Add(new GameInfo
            {
                Id = 105,
                CreatorName = "Капитан",
                StartTime = DateTime.Now,
                Status = "👤 Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2,
                CreatorRating = 1750
            });

            Games.Add(new GameInfo
            {
                Id = 106,
                CreatorName = "Бывалый",
                StartTime = DateTime.Now.AddMinutes(-30),
                Status = "👤 Ожидает игрока",
                PlayersCount = 1,
                MaxPlayers = 2,
                CreatorRating = 1600
            });

            Games.Add(new GameInfo
            {
                Id = 107,
                CreatorName = "Адмирал",
                StartTime = DateTime.Now.AddMinutes(-10),
                Status = "⚔ В процессе",
                PlayersCount = 2,
                MaxPlayers = 2,
                CreatorRating = 2300
            });
        }

        private async Task JoinGame()
        {
            if (SelectedGame == null)
            {
                MessageBox.Show("❌ Выберите игру из списка для присоединения",
                    "Выбор игры", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedGame.Status != "👤 Ожидает игрока")
            {
                MessageBox.Show("❌ Эта игра уже началась или завершена.\nВыберите другую игру из списка.",
                    "Игра недоступна", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            StatusMessage = $"Присоединение к игре #{SelectedGame.Id}...";

            try
            {
                var result = await Client.Instance.PostAsync($"GameInfo/JoinGame?idGame={SelectedGame.Id}");

                if (result.Success)
                {
                    MessageBox.Show($"✅ Вы успешно присоединились к игре!\n\nСоздатель: {SelectedGame.CreatorName}\nID игры: {SelectedGame.Id}\n\nТеперь расставьте свои корабли.",
                        "Присоединение успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Переход на страницу расстановки кораблей
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new ShipPlacementPage();
                }
                else
                {
                    MessageBox.Show($"❌ Ошибка присоединения:\n{result.Response}\n\nВозможно, игра уже началась или была удалена.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠ Исключение при присоединении:\n{ex.Message}",
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