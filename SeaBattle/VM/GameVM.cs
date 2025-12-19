using SeaBattle.mvvm;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeaBattle.VM
{
    public class GameVM : BaseVM
    {
        private string gameStatus = "Ожидание хода";
        public string GameStatus
        {
            get => gameStatus;
            set { gameStatus = value; Signal(); }
        }

        private string currentPlayer = "Вы";
        public string CurrentPlayer
        {
            get => currentPlayer;
            set { currentPlayer = value; Signal(); }
        }

        private Canvas myField;
        private Canvas enemyField;

        public CommandVM ExitCommand { get; set; }

        public GameVM()
        {
            ExitCommand = new CommandVM(() =>
            {
                var pageControl = PageControl.GetInstance();
                pageControl.CurrentPage = new View.PageListGames();
            });
        }

        public void InitializeFields(Canvas myFieldCanvas, Canvas enemyFieldCanvas)
        {
            myField = myFieldCanvas;
            enemyField = enemyFieldCanvas;

            DrawGrid(myField);
            DrawGrid(enemyField);

            // Добавляем тестовые корабли
            DrawTestShips();
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
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(letterText, i * 30 + 10);
                Canvas.SetTop(letterText, -20);
                canvas.Children.Add(letterText);

                // Цифры слева (1-10)
                var numberText = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(numberText, -20);
                Canvas.SetTop(numberText, i * 30 + 10);
                canvas.Children.Add(numberText);
            }
        }

        private void DrawTestShips()
        {
            if (myField == null) return;

            // Тестовый корабль 1x4
            DrawShip(2, 2, 4, true, Brushes.Gray);

            // Тестовый корабль 1x3
            DrawShip(5, 5, 3, false, Brushes.Gray);

            // Тестовый корабль 1x2
            DrawShip(8, 2, 2, true, Brushes.Gray);

            // Тестовый корабль 1x1
            DrawShip(0, 8, 1, true, Brushes.Gray);
        }

        private void DrawShip(int x, int y, int size, bool horizontal, Brush color)
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

                myField.Children.Add(shipCell);
            }
        }
    }
}