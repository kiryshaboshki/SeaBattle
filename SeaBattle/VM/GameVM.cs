using SeaBattle.API;
using SeaBattle.mvvm;
using SeaBattle.Models;
using SeaBattle.View;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SeaBattle.VM
{
    public class GameVM : BaseVM
    {
        private string gameStatus = "Подключение к серверу...";
        public string GameStatus
        {
            get => gameStatus;
            set { gameStatus = value; Signal(); }
        }

        private string currentPlayer = "Ожидание сервера";
        public string CurrentPlayer
        {
            get => currentPlayer;
            set { currentPlayer = value; Signal(); }
        }

        private string turnInfo = "Инициализация игры...";
        public string TurnInfo
        {
            get => turnInfo;
            set { turnInfo = value; Signal(); }
        }

        private int myShipsAlive = 10;
        public int MyShipsAlive
        {
            get => myShipsAlive;
            set { myShipsAlive = value; Signal(); }
        }

        private int enemyShipsAlive = 10;
        public int EnemyShipsAlive
        {
            get => enemyShipsAlive;
            set { enemyShipsAlive = value; Signal(); }
        }

        private int myShipsTotal = 10;
        public int MyShipsTotal
        {
            get => myShipsTotal;
            set { myShipsTotal = value; Signal(); }
        }

        private int enemyShipsTotal = 10;
        public int EnemyShipsTotal
        {
            get => enemyShipsTotal;
            set { enemyShipsTotal = value; Signal(); }
        }

        private bool isMyTurn = false;
        public bool IsMyTurn
        {
            get => isMyTurn;
            set
            {
                isMyTurn = value;
                Signal();
                UpdateTurnInfo();
            }
        }

        private bool isConnected = false;
        public bool IsConnected
        {
            get => isConnected;
            set { isConnected = value; Signal(); }
        }

        private bool isLoading = false;
        public bool IsLoading
        {
            get => isLoading;
            set { isLoading = value; Signal(); }
        }

        private string connectionStatus = "⏳ Подключение...";
        public string ConnectionStatus
        {
            get => connectionStatus;
            set { connectionStatus = value; Signal(); }
        }

        private ObservableCollection<string> gameLog = new ObservableCollection<string>();
        public ObservableCollection<string> GameLog
        {
            get => gameLog;
            set { gameLog = value; Signal(); }
        }

        private Canvas myField;
        private Canvas enemyField;
        private DispatcherTimer updateTimer;
        private Random random = new Random();

        public CommandVM ExitCommand { get; set; }
        public CommandVM SurrenderCommand { get; set; }
        public CommandVM RefreshCommand { get; set; }

        public GameVM()
        {
            ExitCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти из игры?",
                    "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    StopUpdateTimer();
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new PageListGames();
                }
            });

            SurrenderCommand = new CommandVM(async () =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите сдаться?",
                    "Подтверждение сдачи", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    AddToLog("Вы сдались. Отправка на сервер...");

                    if (CurrentGame.IsOnline)
                    {
                        try
                        {
                            var apiResult = await Client.Instance.PostAsync($"Game/Surrender/{CurrentGame.Id}");
                            if (apiResult.Success)
                            {
                                AddToLog("Сдача зарегистрирована на сервере");
                            }
                        }
                        catch { }
                    }

                    AddToLog("Вы сдались. Поражение!");
                    MessageBox.Show("Вы сдались. Поражение!", "Сдача",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    StopUpdateTimer();
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new PageListGames();
                }
            });

            RefreshCommand = new CommandVM(async () =>
            {
                await CheckServerConnection();
            });

            InitializeGame();
            StartUpdateTimer();
        }

        private void InitializeGame()
        {
            MyShipsTotal = 10;
            EnemyShipsTotal = 10;
            MyShipsAlive = 10;
            EnemyShipsAlive = 10;

            if (CurrentGame.IsOnline)
            {
                GameStatus = "Подключение к игровому серверу...";
                ConnectionStatus = "🌐 Онлайн";
            }
            else
            {
                GameStatus = "Оффлайн режим - игра с ИИ";
                ConnectionStatus = "📴 Оффлайн";
            }

            GameLog.Clear();
            AddToLog("Игра инициализирована");
            AddToLog(CurrentGame.IsOnline ?
                $"Игра #{CurrentGame.Id} против {CurrentGame.OpponentName}" :
                "Оффлайн игра с компьютером");

            UpdateTurnInfo();
        }

        private void UpdateTurnInfo()
        {
            if (IsMyTurn)
            {
                CurrentPlayer = "ВАШ ХОД";
                TurnInfo = "Кликайте по полю противника для выстрела";
                GameStatus = "Ваша очередь стрелять";
            }
            else
            {
                CurrentPlayer = "ХОД ПРОТИВНИКА";
                TurnInfo = CurrentGame.IsOnline ?
                    "Ожидание хода противника..." :
                    "Компьютер думает...";
                GameStatus = CurrentGame.IsOnline ?
                    "Противник делает ход" :
                    "Ход компьютера";
            }
        }

        public void InitializeFields(Canvas myFieldCanvas, Canvas enemyFieldCanvas)
        {
            myField = myFieldCanvas;
            enemyField = enemyFieldCanvas;

            DrawGrid(myField);
            DrawGrid(enemyField);
            DrawShipsOnMyField();

            AddToLog("Поля инициализированы");

            // Пытаемся подключиться к серверу
            if (CurrentGame.IsOnline)
            {
                Task.Run(async () => await CheckServerConnection());
            }
            else
            {
                IsMyTurn = true; // В оффлайн режиме начинаем первыми
            }
        }

        private void DrawShipsOnMyField()
        {
            if (myField == null) return;

            // Очищаем старые корабли
            var childrenToRemove = new System.Collections.Generic.List<UIElement>();
            foreach (UIElement child in myField.Children)
            {
                if (child is Rectangle rect && rect.Fill != Brushes.Red && rect.Fill != Brushes.Blue)
                {
                    childrenToRemove.Add(child);
                }
            }
            foreach (var child in childrenToRemove)
            {
                myField.Children.Remove(child);
            }

            // Здесь в реальном приложении получаем расстановку с сервера
            // Пока рисуем тестовые корабли
            DrawTestShips();
        }

        private void DrawTestShips()
        {
            // Тестовые корабли (как в ТЗ)
            DrawShip(1, 1, 4, true, Brushes.DarkGray, myField);
            DrawShip(6, 1, 3, false, Brushes.DarkGray, myField);
            DrawShip(1, 6, 3, true, Brushes.DarkGray, myField);
            DrawShip(8, 3, 2, false, Brushes.DarkGray, myField);
            DrawShip(4, 8, 2, true, Brushes.DarkGray, myField);
            DrawShip(0, 0, 2, false, Brushes.DarkGray, myField);
            DrawSingleShip(9, 9, Brushes.DarkGray, myField);
            DrawSingleShip(5, 5, Brushes.DarkGray, myField);
            DrawSingleShip(2, 9, Brushes.DarkGray, myField);
            DrawSingleShip(9, 2, Brushes.DarkGray, myField);
        }

        private void DrawGrid(Canvas canvas)
        {
            // (оставляем ваш существующий код DrawGrid)
            canvas.Children.Clear();

            for (int i = 0; i <= 10; i++)
            {
                var verticalLine = new Line
                {
                    X1 = i * 30,
                    Y1 = 0,
                    X2 = i * 30,
                    Y2 = 300,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(verticalLine);

                var horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = i * 30,
                    X2 = 300,
                    Y2 = i * 30,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };
                canvas.Children.Add(horizontalLine);
            }

            for (int i = 0; i < 10; i++)
            {
                var letterText = new TextBlock
                {
                    Text = ((char)('A' + i)).ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(letterText, i * 30 + 10);
                Canvas.SetTop(letterText, -20);
                canvas.Children.Add(letterText);

                var numberText = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(numberText, -20);
                Canvas.SetTop(numberText, i * 30 + 10);
                canvas.Children.Add(numberText);
            }
        }

        private void DrawShip(int x, int y, int size, bool horizontal, Brush color, Canvas canvas)
        {
            for (int i = 0; i < size; i++)
            {
                var shipCell = new Rectangle
                {
                    Width = 28,
                    Height = 28,
                    Fill = color,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                if (horizontal)
                {
                    Canvas.SetLeft(shipCell, (x + i) * 30 + 1);
                    Canvas.SetTop(shipCell, y * 30 + 1);
                }
                else
                {
                    Canvas.SetLeft(shipCell, x * 30 + 1);
                    Canvas.SetTop(shipCell, (y + i) * 30 + 1);
                }

                canvas.Children.Add(shipCell);
            }
        }

        private void DrawSingleShip(int x, int y, Brush color, Canvas canvas)
        {
            var shipCell = new Rectangle
            {
                Width = 28,
                Height = 28,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(shipCell, x * 30 + 1);
            Canvas.SetTop(shipCell, y * 30 + 1);
            canvas.Children.Add(shipCell);
        }

        public async Task ProcessShot(int x, int y, bool isMyShot)
        {
            if (!IsMyTurn && isMyShot)
            {
                MessageBox.Show("Сейчас не ваш ход! Ожидайте хода противника.",
                    "Не ваш ход", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string cellName = $"{(char)('A' + x)}{y + 1}";

            if (isMyShot)
            {
                AddToLog($"Ваш выстрел в {cellName}...");
                IsLoading = true;

                if (CurrentGame.IsOnline)
                {
                    // Реальный вызов к серверу
                    await ProcessOnlineShot(x, y, cellName);
                }
                else
                {
                    // Оффлайн режим - случайный результат
                    await ProcessOfflineShot(x, y, cellName);
                }

                IsLoading = false;
                CheckGameEnd();
            }
        }

        private async Task ProcessOnlineShot(int x, int y, string cellName)
        {
            try
            {
                var shotResult = await GameAPI.SendShot(x, y);

                if (shotResult.Success)
                {
                    if (shotResult.IsHit)
                    {
                        DrawHitMarker(x, y, Brushes.DarkRed, enemyField);
                        EnemyShipsAlive--;

                        if (shotResult.IsDestroyed)
                        {
                            AddToLog($"✓ УНИЧТОЖЕН корабль в {cellName}! Размер: {shotResult.ShipSize}");
                            MessageBox.Show($"УНИЧТОЖЕН корабль! {cellName}\nУ противника осталось: {EnemyShipsAlive}",
                                "Уничтожение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }
                        else
                        {
                            AddToLog($"✓ ПОПАДАНИЕ в {cellName}! У противника осталось: {EnemyShipsAlive}");
                            MessageBox.Show($"ПОПАДАНИЕ! {cellName}\nУ противника осталось: {EnemyShipsAlive}",
                                "Попадание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        }

                        // Дополнительный ход при попадании
                        IsMyTurn = true;
                        AddToLog("Дополнительный ход за попадание!");
                    }
                    else
                    {
                        DrawMissMarker(x, y, Brushes.Blue, enemyField);
                        AddToLog($"✗ Промах в {cellName}");
                        MessageBox.Show($"Промах! {cellName}",
                            "Промах", MessageBoxButton.OK, MessageBoxImage.Information);
                        IsMyTurn = false;
                    }
                }
                else
                {
                    AddToLog($"Ошибка выстрела: {shotResult.Message}");
                    MessageBox.Show($"Ошибка: {shotResult.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                AddToLog($"Сетевая ошибка: {ex.Message}");
                MessageBox.Show($"Ошибка соединения: {ex.Message}\nПереход в оффлайн-режим.",
                    "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Warning);
                CurrentGame.IsOnline = false;
                ConnectionStatus = "📴 Оффлайн";
            }
        }

        private async Task ProcessOfflineShot(int x, int y, string cellName)
        {
            // Имитация задержки сети
            await Task.Delay(500);

            bool isHit = random.Next(0, 2) == 0;

            if (isHit)
            {
                DrawHitMarker(x, y, Brushes.DarkRed, enemyField);
                EnemyShipsAlive--;
                AddToLog($"✓ ПОПАДАНИЕ в {cellName}! У противника осталось: {EnemyShipsAlive}");
                MessageBox.Show($"ПОПАДАНИЕ! {cellName}\nУ противника осталось: {EnemyShipsAlive}",
                    "Попадание", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                IsMyTurn = true;
                AddToLog("Дополнительный ход за попадание!");
            }
            else
            {
                DrawMissMarker(x, y, Brushes.Blue, enemyField);
                AddToLog($"✗ Промах в {cellName}");
                MessageBox.Show($"Промах! {cellName}",
                    "Промах", MessageBoxButton.OK, MessageBoxImage.Information);
                IsMyTurn = false;

                // Имитация хода компьютера
                if (!IsMyTurn)
                {
                    await Task.Delay(1000);
                    SimulateEnemyTurn();
                }
            }
        }

        private void DrawHitMarker(int x, int y, Brush color, Canvas canvas)
        {
            var hitMarker = new Ellipse
            {
                Width = 24,
                Height = 24,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Canvas.SetLeft(hitMarker, x * 30 + 3);
            Canvas.SetTop(hitMarker, y * 30 + 3);
            canvas.Children.Add(hitMarker);
        }

        private void DrawMissMarker(int x, int y, Brush color, Canvas canvas)
        {
            var missMarker = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Opacity = 0.7
            };

            Canvas.SetLeft(missMarker, x * 30 + 5);
            Canvas.SetTop(missMarker, y * 30 + 5);
            canvas.Children.Add(missMarker);
        }

        private void CheckGameEnd()
        {
            if (EnemyShipsAlive <= 0)
            {
                AddToLog("★★★★★ ПОБЕДА! Все корабли противника уничтожены! ★★★★★");
                MessageBox.Show("ПОБЕДА! 🏆\nВы уничтожили все корабли противника!",
                    "Победа", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                GameStatus = "Игра завершена - ВЫ ПОБЕДИЛИ! 🎉";
                IsMyTurn = false;
                StopUpdateTimer();
            }
            else if (MyShipsAlive <= 0)
            {
                AddToLog("☠☠☠ ПОРАЖЕНИЕ! Все ваши корабли уничтожены ☠☠☠");
                MessageBox.Show("ПОРАЖЕНИЕ! ☠\nВсе ваши корабли уничтожены.",
                    "Поражение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                GameStatus = "Игра завершена - ВЫ ПРОИГРАЛИ";
                IsMyTurn = false;
                StopUpdateTimer();
            }
        }

        private void AddToLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            Application.Current.Dispatcher.Invoke(() =>
            {
                GameLog.Add($"[{timestamp}] {message}");
                if (GameLog.Count > 20) GameLog.RemoveAt(0);
            });
        }

        public async void SimulateEnemyTurn()
        {
            if (!IsMyTurn)
            {
                AddToLog("Противник делает ход...");

                await Task.Delay(1000);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    int x = random.Next(0, 10);
                    int y = random.Next(0, 10);
                    string cellName = $"{(char)('A' + x)}{y + 1}";

                    bool isHit = random.Next(0, 2) == 0;

                    if (isHit)
                    {
                        DrawHitMarker(x, y, Brushes.DarkOrange, myField);
                        MyShipsAlive--;
                        AddToLog($"☠ Противник попал в {cellName}! Ваших кораблей осталось: {MyShipsAlive}");
                    }
                    else
                    {
                        DrawMissMarker(x, y, Brushes.LightBlue, myField);
                        AddToLog($"◯ Противник промахнулся в {cellName}");
                    }

                    IsMyTurn = true;
                    AddToLog("Ваш ход!");
                    CheckGameEnd();
                });
            }
        }

        private void StartUpdateTimer()
        {
            updateTimer = new DispatcherTimer();
            updateTimer.Interval = TimeSpan.FromSeconds(5); // Опрос каждые 5 секунд
            updateTimer.Tick += async (s, e) => await CheckForServerUpdates();
            updateTimer.Start();
        }

        private void StopUpdateTimer()
        {
            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer = null;
            }
        }

        private async Task CheckForServerUpdates()
        {
            if (!CurrentGame.IsOnline || !isConnected || IsLoading) return;

            try
            {
                var hasUpdates = await GameAPI.CheckForUpdates();
                if (hasUpdates)
                {
                    await UpdateGameStateFromServer();
                }
            }
            catch
            {
                // Игнорируем ошибки таймера
            }
        }

        private async Task UpdateGameStateFromServer()
        {
            try
            {
                var gameState = await GameAPI.GetGameState();
                if (gameState != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsMyTurn = gameState.IsMyTurn;
                        MyShipsAlive = gameState.MyShipsAlive;
                        EnemyShipsAlive = gameState.EnemyShipsAlive;
                        GameStatus = gameState.GameStatus;

                        if (!IsMyTurn)
                        {
                            // Если сейчас ход противника, получаем его ход
                            Task.Run(async () => await GetEnemyTurnFromServer());
                        }
                    });
                }
            }
            catch { }
        }

        private async Task GetEnemyTurnFromServer()
        {
            try
            {
                var enemyTurn = await GameAPI.GetEnemyTurn();
                if (enemyTurn.Success)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        string cellName = $"{(char)('A' + enemyTurn.X)}{enemyTurn.Y + 1}";

                        if (enemyTurn.IsHit)
                        {
                            DrawHitMarker(enemyTurn.X, enemyTurn.Y, Brushes.DarkOrange, myField);
                            MyShipsAlive--;
                            AddToLog($"☠ Противник попал в {cellName}! Осталось: {MyShipsAlive}");
                        }
                        else
                        {
                            DrawMissMarker(enemyTurn.X, enemyTurn.Y, Brushes.LightBlue, myField);
                            AddToLog($"◯ Противник промахнулся в {cellName}");
                        }

                        IsMyTurn = true;
                        AddToLog("Ваш ход!");
                        CheckGameEnd();
                    });
                }
            }
            catch { }
        }

        private async Task CheckServerConnection()
        {
            if (!CurrentGame.IsOnline) return;

            IsLoading = true;
            ConnectionStatus = "⏳ Проверка связи...";

            try
            {
                var result = await Client.Instance.PostAsync("Game/Ping");
                IsConnected = result.Success;
                ConnectionStatus = result.Success ? "🌐 Онлайн" : "⚠️ Сервер недоступен";

                if (result.Success)
                {
                    AddToLog("Соединение с сервером установлено");
                    await UpdateGameStateFromServer();
                }
                else
                {
                    AddToLog("Сервер недоступен");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ConnectionStatus = "❌ Ошибка подключения";
                AddToLog($"Ошибка подключения: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}