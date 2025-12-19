using SeaBattleRepository.DTO;
using SeaBattleWPF.API;
using SeaBattleWPF.API.Game;
using SeaBattleWPF.VM;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SeaBattleWPF.View
{
    public partial class LoginPage : Page
    {
        private TcpGameClient _client;

        public LoginPage()
        {
            InitializeComponent();

            _client = TcpGameClient.Instance;

            // Подписываемся на события
            _client.OnAuthSuccess += OnAuthSuccess;
            _client.OnError += OnError;

            // По умолчанию онлайн режим
            OnlineModeRadio.IsChecked = true;
            UpdateUIForMode();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Уже вызвано в конструкторе
        }

        private void GameMode_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUIForMode();
        }

        private void UpdateUIForMode()
        {
            if (OnlineModeRadio.IsChecked == true)
            {
                _client.SetGameMode(GameMode.Online);
                OnlinePanel.Visibility = Visibility.Visible;
                OfflinePanel.Visibility = Visibility.Collapsed;
                TestPanel.Visibility = Visibility.Collapsed;
                QuickStartButton.Visibility = Visibility.Collapsed;
            }
            else if (OfflineModeRadio.IsChecked == true)
            {
                _client.SetGameMode(GameMode.OfflineAI);
                OnlinePanel.Visibility = Visibility.Collapsed;
                OfflinePanel.Visibility = Visibility.Visible;
                TestPanel.Visibility = Visibility.Collapsed;
                QuickStartButton.Visibility = Visibility.Visible;
            }
            else if (TestModeRadio.IsChecked == true)
            {
                _client.SetGameMode(GameMode.Test);
                OnlinePanel.Visibility = Visibility.Collapsed;
                OfflinePanel.Visibility = Visibility.Collapsed;
                TestPanel.Visibility = Visibility.Visible;
                QuickStartButton.Visibility = Visibility.Visible;
            }
        }

        private void OnAuthSuccess(UserDTO user)
        {
            Dispatcher.Invoke(() =>
            {
                PageControl.GetInstance().CurrentPage = new PageListGames();
            });
        }

        private void OnError(string error)
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = error;
            });
        }

        // === ОНЛАЙН РЕЖИМ ===
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PortTextBox.Text, out int port))
            {
                ConnectionStatus.Text = "Некорректный порт";
                ConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            ConnectButton.IsEnabled = false;
            ConnectionStatus.Text = "Подключение...";
            ConnectionStatus.Foreground = System.Windows.Media.Brushes.Black;

            try
            {
                var connected = await _client.ConnectAsync(IpTextBox.Text, port);
                if (connected)
                {
                    ConnectionStatus.Text = "Подключено";
                    ConnectionStatus.Foreground = System.Windows.Media.Brushes.Green;
                    LoginButton.IsEnabled = true;
                    RegisterButton.IsEnabled = true;
                }
                else
                {
                    ConnectionStatus.Text = "Ошибка подключения";
                    ConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            finally
            {
                ConnectButton.IsEnabled = true;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                StatusText.Text = "Введите логин и пароль";
                return;
            }

            if (!_client.IsConnected && OnlineModeRadio.IsChecked == true)
            {
                StatusText.Text = "Сначала подключитесь к серверу";
                return;
            }

            var authData = new
            {
                Login = LoginTextBox.Text,
                Password = PasswordBox.Password
            };

            _client.SendAsync("AUTH", authData);
            StatusText.Text = "Авторизация...";
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                StatusText.Text = "Введите логин и пароль";
                return;
            }

            if (!_client.IsConnected && OnlineModeRadio.IsChecked == true)
            {
                StatusText.Text = "Сначала подключитесь к серверу";
                return;
            }

            var regData = new
            {
                Login = LoginTextBox.Text,
                Password = PasswordBox.Password
            };

            _client.SendAsync("REGISTER", regData);
            StatusText.Text = "Регистрация...";
        }

        private void QuickStartButton_Click(object sender, RoutedEventArgs e)
        {
            // Быстрый старт без авторизации
            StartOfflineGameDirectly();
        }

        // === ОФФЛАЙН РЕЖИМ ===
        private void StartOfflineGameButton_Click(object sender, RoutedEventArgs e)
        {
            StartOfflineGameDirectly();
        }

        private void StartOfflineGameDirectly()
        {
            try
            {
                // Устанавливаем оффлайн режим
                var client = TcpGameClient.Instance;
                client.SetGameMode(GameMode.OfflineAI);

                // Создаём игру напрямую без эмуляции авторизации
                Task.Delay(100).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Создаём оффлайн игру
                        var offlineGame = new GameDTO
                        {
                            Id = 999, // Специальный ID для оффлайн игр
                            Creator = new UserDTO
                            {
                                Id = 1,
                                Login = LoginTextBox.Text ?? "Оффлайн игрок",
                                Password = PasswordBox.Password ?? "",
                                Rating = 1000
                            },
                            Opponent = new UserDTO
                            {
                                Id = 2,
                                Login = "Компьютер (ИИ)",
                                Password = "",
                                Rating = 900
                            },
                            Status = 1, // Статус "в процессе"
                            IdUserNextTurn = 1, // Игрок ходит первым
                            DatetimeStartGame = DateTime.Now,
                            FieldUser1 = new byte[100], // Поле игрока (пока пустое)
                            FieldUser2 = new byte[100]  // Поле ИИ (пока пустое)
                        };

                        // Инициализируем глобальное состояние
                        Game.CurrentGame = offlineGame;
                        Game.CreatorIsCurrentUser = true;

                        // СОЗДАЁМ ИГРУ ПРЯМО ЗДЕСЬ БЕЗ ShipPlacementPage
                        // Генерируем случайное поле для игрока
                        var gameVM = new GameVM();
                        byte[] playerField = gameVM.GenerateRandomField();

                        // Сохраняем поле в игре
                        offlineGame.FieldUser1 = playerField;

                        // Переходим на игровую страницу
                        var gamePage = new GamePage();
                        gamePage.DataContext = gameVM;
                        PageControl.GetInstance().CurrentPage = gamePage;

                        // Инициализируем GamePage
                        gameVM.RegisterDispatcher(gamePage.Dispatcher);
                        gameVM.RegisterField(gamePage.FieldUser1, true);
                        gameVM.RegisterField(gamePage.FieldUser2, false);

                        // Устанавливаем поле игрока и перерисовываем
                        gameVM.SetPlayerField(playerField);

                        StatusText.Text = "Оффлайн игра создана. Ваш ход!";
                        MessageBox.Show("Игра против ИИ начата!\n\nВаши корабли расставлены случайным образом.\n\nКликайте по правому полю, чтобы сделать ход.",
                                       "Оффлайн игра", MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Ошибка: {ex.Message}";
                MessageBox.Show($"Не удалось начать оффлайн игру:\n{ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // === ТЕСТОВЫЙ РЕЖИМ ===
        private void TestFieldButton_Click(object sender, RoutedEventArgs e)
        {
            // Переходим к тестовой странице
            var gamePage = new GamePage();
            PageControl.GetInstance().CurrentPage = gamePage;

            // Создаём тестовое поле
            var testField = CreateTestField();

            // Отрисовываем на первом поле через Game.RedrawMyField
            Game.RedrawMyField(gamePage.FieldUser1, testField);
        }

        private void TestGameButton_Click(object sender, RoutedEventArgs e)
        {
            // Просто переходим к игровой странице
            PageControl.GetInstance().CurrentPage = new GamePage();
        }

        private void TestShipsButton_Click(object sender, RoutedEventArgs e)
        {
            // Переходим к тестовой странице
            var gamePage = new GamePage();
            PageControl.GetInstance().CurrentPage = gamePage;

            // Создаём тестовое поле с кораблями
            var shipsField = CreateShipsTestField();

            // Отрисовываем на первом поле
            Game.RedrawMyField(gamePage.FieldUser1, shipsField);
        }

        private byte[] CreateTestField()
        {
            var field = new byte[100];

            // Простое тестовое поле
            for (int i = 0; i < 100; i++)
            {
                if (i % 4 == 0) field[i] = 1; // Корабли
                else if (i % 5 == 0) field[i] = 2; // Попадания
                else if (i % 6 == 0) field[i] = 3; // Промахи
                else field[i] = 0; // Море
            }

            return field;
        }

        private byte[] CreateShipsTestField()
        {
            var field = new byte[100];

            // Простая расстановка кораблей для теста
            // 4-палубный
            field[5] = 1;
            field[6] = 1;
            field[7] = 1;
            field[8] = 1;

            // 3-палубные
            field[22] = 1;
            field[32] = 1;
            field[42] = 1;

            field[55] = 1;
            field[56] = 1;
            field[57] = 1;

            return field;
        }

        private void BackToModeSelectButton_Click(object sender, RoutedEventArgs e)
        {
            OnlineModeRadio.IsChecked = true;
            UpdateUIForMode();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_client != null)
            {
                _client.OnAuthSuccess -= OnAuthSuccess;
                _client.OnError -= OnError;
            }
        }
    }
}