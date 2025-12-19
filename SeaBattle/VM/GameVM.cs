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

namespace SeaBattle.VM
{
    public class GameVM : BaseVM
    {
        private string gameStatus = "Ожидание подключения противника";
        public string GameStatus
        {
            get => gameStatus;
            set { gameStatus = value; Signal(); }
        }

        private string currentPlayer = "Ваш ход";
        public string CurrentPlayer
        {
            get => currentPlayer;
            set { currentPlayer = value; Signal(); }
        }

        private string turnInfo = "Сделайте выстрел по полю противника";
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

        private bool isMyTurn = true;
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

        private ObservableCollection<string> gameLog = new ObservableCollection<string>();
        public ObservableCollection<string> GameLog
        {
            get => gameLog;
            set { gameLog = value; Signal(); }
        }

        private Canvas myField;
        private Canvas enemyField;
        private Random random = new Random();

        public CommandVM ExitCommand { get; set; }
        public CommandVM SurrenderCommand { get; set; }

        public GameVM()
        {
            ExitCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти из игры?",
                    "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
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
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new PageListGames();
                }
            });

            // Инициализируем начальные значения
            InitializeGame();
        }

        private void InitializeGame()
        {
            MyShipsTotal = 10;  // 1x4 + 2x3 + 3x2 + 4x1 = 10 кораблей
            EnemyShipsTotal = 10;
            MyShipsAlive = 10;
            EnemyShipsAlive = 10;
            GameStatus = "Расстановка завершена. Ожидание хода...";
            GameLog.Clear();
            AddToLog("Игра началась. Расставьте корабли.");
            AddToLog("Ожидание подключения противника...");
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
                TurnInfo = "Ожидание хода противника...";
                GameStatus = "Противник делает ход";
            }
        }

        public void InitializeFields(Canvas myFieldCanvas, Canvas enemyFieldCanvas)
        {
            myField = myFieldCanvas;
            enemyField = enemyFieldCanvas;

            DrawGrid(myField);
            DrawGrid(enemyField);

            // Для демонстрации - отрисовываем тестовые корабли на своем поле
            DrawTestShipsOnMyField();
            AddToLog("Поля инициализированы. Игра готова.");
        }

        private void DrawGrid(Canvas canvas)
        {
            canvas.Children.Clear();

            // Рисуем сетку 10x10
            for (int i = 0; i <= 10; i++)
            {
                // Вертикальные линии
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

                // Горизонтальные линии
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

            // Добавляем координаты
            for (int i = 0; i < 10; i++)
            {
                // Буквы сверху (A-J)
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

                // Цифры слева (1-10)
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

        private void DrawTestShipsOnMyField()
        {
            if (myField == null) return;

            // Очищаем поле
            var childrenToRemove = new System.Collections.Generic.List<UIElement>();
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

            // Стандартные корабли для морского боя
            // 1 корабль на 4 клетки
            DrawShip(1, 1, 4, true, Brushes.DarkGray, myField);

            // 2 корабля на 3 клетки
            DrawShip(6, 1, 3, false, Brushes.DarkGray, myField);
            DrawShip(1, 6, 3, true, Brushes.DarkGray, myField);

            // 3 корабля на 2 клетки
            DrawShip(8, 3, 2, false, Brushes.DarkGray, myField);
            DrawShip(4, 8, 2, true, Brushes.DarkGray, myField);
            DrawShip(0, 0, 2, false, Brushes.DarkGray, myField);

            // 4 корабля на 1 клетку
            DrawSingleShip(9, 9, Brushes.DarkGray, myField);
            DrawSingleShip(5, 5, Brushes.DarkGray, myField);
            DrawSingleShip(2, 9, Brushes.DarkGray, myField);
            DrawSingleShip(9, 2, Brushes.DarkGray, myField);
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

        public void ProcessShot(int x, int y, bool isMyShot)
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

                // Имитация попадания/промаха (в реальном приложении - ответ сервера)
                bool isHit = random.Next(0, 2) == 0;

                if (isHit)
                {
                    DrawHitMarker(x, y, Brushes.DarkRed, enemyField);
                    EnemyShipsAlive--;
                    AddToLog($"✓ ПОПАДАНИЕ в {cellName}! У противника осталось кораблей: {EnemyShipsAlive}");
                    MessageBox.Show($"ПОПАДАНИЕ! {cellName}\nУ противника осталось кораблей: {EnemyShipsAlive}",
                        "Попадание", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                    // Дополнительный ход при попадании (правила морского боя)
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

                // Проверка конца игры
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
            }
            else if (MyShipsAlive <= 0)
            {
                AddToLog("☠☠☠ ПОРАЖЕНИЕ! Все ваши корабли уничтожены ☠☠☠");
                MessageBox.Show("ПОРАЖЕНИЕ! ☠\nВсе ваши корабли уничтожены.",
                    "Поражение", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                GameStatus = "Игра завершена - ВЫ ПРОИГРАЛИ";
                IsMyTurn = false;
            }
        }

        private void AddToLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            GameLog.Add($"[{timestamp}] {message}");

            // Ограничиваем лог последними 20 сообщениями
            if (GameLog.Count > 20)
            {
                GameLog.RemoveAt(0);
            }
        }

        // Метод для имитации хода противника (для демонстрации)
        public void SimulateEnemyTurn()
        {
            if (!IsMyTurn)
            {
                AddToLog("Противник делает ход...");

                // Имитация задержки
                Task.Delay(1000).ContinueWith(t =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Случайный выстрел противника
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

                        // Возвращаем ход игроку
                        IsMyTurn = true;
                        AddToLog("Ваш ход!");

                        CheckGameEnd();
                    });
                });
            }
        }
    }
}