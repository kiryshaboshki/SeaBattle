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

        private Canvas myField;
        private Canvas enemyField;

        public CommandVM ExitCommand { get; set; }
        public CommandVM SurrenderCommand { get; set; }

        public GameVM()
        {
            ExitCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите выйти из игры?",
                    "Подтверждение выхода", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new PageListGames();
                }
            });

            SurrenderCommand = new CommandVM(() =>
            {
                var result = MessageBox.Show("Вы уверены, что хотите сдаться?",
                    "Подтверждение сдачи", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    MessageBox.Show("Вы сдались. Поражение!");
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
        }

        public void InitializeFields(Canvas myFieldCanvas, Canvas enemyFieldCanvas)
        {
            myField = myFieldCanvas;
            enemyField = enemyFieldCanvas;

            DrawGrid(myField);
            DrawGrid(enemyField);

            // Для демонстрации - отрисовываем тестовые корабли на своем поле
            DrawTestShipsOnMyField();
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
            // Удаляем только корабли, оставляя сетку
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
            // Здесь будет логика обработки выстрела
            // Пока что просто демонстрация
            if (isMyShot)
            {
                // Отрисовываем выстрел на поле противника
                DrawShotMarker(x, y, Brushes.Red, enemyField);
                MessageBox.Show($"Выстрел в {Convert.ToChar('A' + x)}{y + 1}");
            }
        }

        private void DrawShotMarker(int x, int y, Brush color, Canvas canvas)
        {
            var marker = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = color,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };

            Canvas.SetLeft(marker, x * 30 + 5);
            Canvas.SetTop(marker, y * 30 + 5);
            canvas.Children.Add(marker);
        }
    }
}