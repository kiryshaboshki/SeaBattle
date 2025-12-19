using SeaBattle.mvvm;
using SeaBattle.View;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Linq;
using System.Collections.Generic;
using SeaBattle; // ДОБАВИТЬ ЭТОТ USING

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
        private List<Ship> myShips = new List<Ship>();
        private List<Ship> enemyShips = new List<Ship>();

        public CommandVM ExitCommand { get; set; }
        public CommandVM SurrenderCommand { get; set; }
        public CommandVM RefreshCommand { get; set; }

        public GameVM()
        {
            // ПРОВЕРЯЕМ РЕЖИМ ИГРЫ В КОНСТРУКТОРЕ
            if (CurrentGame.IsOnline) // ТЕПЕРЬ ИСПОЛЬЗУЕТСЯ ИЗ SeaBattle
            {
                GameStatus = "Подключение к серверу...";
                ConnectionStatus = "🌐 Онлайн";
            }
            else
            {
                GameStatus = "Оффлайн игра против компьютера";
                ConnectionStatus = "📴 Оффлайн";
                IsMyTurn = true;
            }

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

            SurrenderCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите сдаться?",
                    "Подтверждение сдачи", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    AddToLog("Вы сдались. Поражение!");
                    MessageBox.Show("Вы сдались. Поражение!", "Сдача",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    StopUpdateTimer();
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new PageListGames();
                }
            });

            RefreshCommand = new CommandVM(() =>
            {
                if (!CurrentGame.IsOnline)
                {
                    ConnectionStatus = "📴 Оффлайн";
                    MessageBox.Show("Работаем в оффлайн-режиме", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            InitializeGame();

            if (CurrentGame.IsOnline)
            {
                StartUpdateTimer();
            }
        }

        // ДОБАВИТЬ ЭТОТ МЕТОД (для кнопки в GamePage.xaml.cs)
        public void SimulateEnemyTurn()
        {
            AddToLog("Тестовый ход противника выполнен (функция для отладки)");
        }

        private void InitializeGame()
        {
            MyShipsTotal = 10;
            EnemyShipsTotal = 10;
            MyShipsAlive = 10;
            EnemyShipsAlive = 10;

            if (!CurrentGame.IsOnline)
            {
                GenerateEnemyShips();
            }

            GameLog.Clear();
            AddToLog("Игра инициализирована");
            AddToLog(CurrentGame.IsOnline ?
                $"Игра #{CurrentGame.Id} против {CurrentGame.OpponentName}" :
                "Оффлайн игра с компьютером");

            UpdateTurnInfo();
        }

        private void GenerateEnemyShips()
        {
            enemyShips.Clear();
            var field = new int[10, 10];
            var shipSizes = new[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

            foreach (var size in shipSizes)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    int x = random.Next(0, 10);
                    int y = random.Next(0, 10);
                    bool horizontal = random.Next(0, 2) == 0;

                    if (CanPlaceShip(field, x, y, size, horizontal))
                    {
                        var ship = new Ship
                        {
                            Size = size,
                            IsHorizontal = horizontal,
                            X = x,
                            Y = y,
                            Cells = new List<(int, int)>()
                        };

                        for (int i = 0; i < size; i++)
                        {
                            int cellX = horizontal ? x + i : x;
                            int cellY = horizontal ? y : y + i;
                            field[cellX, cellY] = size;
                            ship.Cells.Add((cellX, cellY));
                        }

                        enemyShips.Add(ship);
                        placed = true;
                    }
                    attempts++;
                }
            }
        }

        private bool CanPlaceShip(int[,] field, int x, int y, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (x + size > 10) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!IsCellAvailable(field, x + i, y)) return false;
                }
            }
            else
            {
                if (y + size > 10) return false;
                for (int i = 0; i < size; i++)
                {
                    if (!IsCellAvailable(field, x, y + i)) return false;
                }
            }
            return true;
        }

        private bool IsCellAvailable(int[,] field, int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;

                    if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                    {
                        if (field[nx, ny] != 0) return false;
                    }
                }
            }
            return true;
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

            if (!CurrentGame.IsOnline)
            {
                IsMyTurn = true;
                AddToLog("Оффлайн игра началась! Ваш ход.");
            }
        }

        private void DrawShipsOnMyField()
        {
            if (myField == null) return;

            var childrenToRemove = new List<UIElement>();
            foreach (UIElement child in myField.Children)
            {
                if (child is Rectangle)
                {
                    childrenToRemove.Add(child);
                }
            }
            foreach (var child in childrenToRemove)
            {
                myField.Children.Remove(child);
            }

            DrawTestShips();
        }

        private void DrawTestShips()
        {
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

                if (!CurrentGame.IsOnline)
                {
                    await ProcessOfflineShot(x, y, cellName);
                }
                else
                {
                    await ProcessOnlineShot(x, y, cellName);
                }

                CheckGameEnd();
            }
        }

        private async Task ProcessOnlineShot(int x, int y, string cellName)
        {
            await Task.Delay(300);

            bool isHit = random.Next(0, 2) == 0;
            bool isDestroyed = isHit && random.Next(0, 3) == 0;

            if (isHit)
            {
                DrawHitMarker(x, y, Brushes.DarkRed, enemyField);
                EnemyShipsAlive--;

                if (isDestroyed)
                {
                    AddToLog($"✓ УНИЧТОЖЕН корабль в {cellName}!");
                    MessageBox.Show($"УНИЧТОЖЕН корабль! {cellName}", "Уничтожение",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    AddToLog($"✓ ПОПАДАНИЕ в {cellName}!");
                    MessageBox.Show($"ПОПАДАНИЕ! {cellName}", "Попадание",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                IsMyTurn = true;
                AddToLog("Дополнительный ход за попадание!");
            }
            else
            {
                DrawMissMarker(x, y, Brushes.Blue, enemyField);
                AddToLog($"✗ Промах в {cellName}");
                MessageBox.Show($"Промах! {cellName}", "Промах",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                IsMyTurn = false;
            }
        }

        private async Task ProcessOfflineShot(int x, int y, string cellName)
        {
            await Task.Delay(300);

            bool isHit = false;
            Ship hitShip = null;

            foreach (var ship in enemyShips)
            {
                if (ship.Cells.Contains((x, y)) && !ship.HitCells.Contains((x, y)))
                {
                    isHit = true;
                    hitShip = ship;
                    ship.HitCells.Add((x, y));
                    break;
                }
            }

            if (isHit)
            {
                DrawHitMarker(x, y, Brushes.DarkRed, enemyField);

                bool isDestroyed = hitShip.IsDestroyed;

                if (isDestroyed)
                {
                    EnemyShipsAlive--;
                    AddToLog($"✓ УНИЧТОЖЕН {hitShip.Size}-палубный корабль в {cellName}!");
                    MessageBox.Show($"УНИЧТОЖЕН корабль! {cellName}\nОсталось кораблей противника: {EnemyShipsAlive}",
                        "Уничтожение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    AddToLog($"✓ ПОПАДАНИЕ в {cellName}!");
                    MessageBox.Show($"ПОПАДАНИЕ! {cellName}", "Попадание",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

                IsMyTurn = true;
                if (!isDestroyed)
                {
                    AddToLog("Дополнительный ход за попадание!");
                }
            }
            else
            {
                DrawMissMarker(x, y, Brushes.Blue, enemyField);
                AddToLog($"✗ Промах в {cellName}");
                MessageBox.Show($"Промах! {cellName}", "Промах",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                IsMyTurn = false;

                if (!IsMyTurn)
                {
                    await Task.Delay(800);
                    await ComputerTurn();
                }
            }
        }

        private async Task ComputerTurn()
        {
            if (!IsMyTurn)
            {
                AddToLog("Компьютер делает ход...");

                await Task.Delay(800);

                int x, y;
                string cellName;
                bool validShot = false;
                int attempts = 0;

                do
                {
                    x = random.Next(0, 10);
                    y = random.Next(0, 10);
                    cellName = $"{(char)('A' + x)}{y + 1}";

                    validShot = true;
                    foreach (UIElement child in myField.Children)
                    {
                        if (child is Ellipse ellipse)
                        {
                            double left = Canvas.GetLeft(ellipse);
                            double top = Canvas.GetTop(ellipse);
                            int shotX = (int)((left - 3) / 30);
                            int shotY = (int)((top - 3) / 30);

                            if (shotX == x && shotY == y)
                            {
                                validShot = false;
                                break;
                            }
                        }
                    }

                    attempts++;
                    if (attempts > 50) break;

                } while (!validShot);

                bool isHit = random.Next(0, 2) == 0;

                if (isHit)
                {
                    DrawHitMarker(x, y, Brushes.DarkOrange, myField);
                    MyShipsAlive--;
                    AddToLog($"☠ Компьютер попал в {cellName}! Осталось кораблей: {MyShipsAlive}");
                    MessageBox.Show($"Компьютер попал в {cellName}!\nВаших кораблей осталось: {MyShipsAlive}",
                        "Попадание компьютера", MessageBoxButton.OK, MessageBoxImage.Warning);

                    await Task.Delay(800);
                    await ComputerTurn();
                }
                else
                {
                    DrawMissMarker(x, y, Brushes.LightBlue, myField);
                    AddToLog($"◯ Компьютер промахнулся в {cellName}");
                    IsMyTurn = true;
                    AddToLog("Ваш ход!");
                }

                CheckGameEnd();
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

        private void StartUpdateTimer()
        {
            if (CurrentGame.IsOnline)
            {
                updateTimer = new DispatcherTimer();
                updateTimer.Interval = TimeSpan.FromSeconds(5);
                updateTimer.Tick += async (s, e) => await CheckForServerUpdates();
                updateTimer.Start();
            }
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
            if (!CurrentGame.IsOnline) return;
            await Task.Delay(100);
        }
    }

    // Внутренний класс для кораблей (оставить)
    public class Ship
    {
        public int Size { get; set; }
        public bool IsHorizontal { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public List<(int, int)> Cells { get; set; } = new List<(int, int)>();
        public List<(int, int)> HitCells { get; set; } = new List<(int, int)>();

        public bool IsDestroyed => HitCells.Count == Size;
    }


}