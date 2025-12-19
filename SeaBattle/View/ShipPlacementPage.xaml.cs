using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SeaBattle.View
{
    public partial class ShipPlacementPage : Page
    {
        private List<Rectangle> shipPreviews = new List<Rectangle>();

        public ShipPlacementPage()
        {
            InitializeComponent();
            DrawGrid();
        }

        private void DrawGrid()
        {
            PlacementField.Children.Clear();
            shipPreviews.Clear();

            // Рисуем сетку
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

                var horizontalLine = new Line
                {
                    X1 = 0,
                    Y1 = i * 30,
                    X2 = 300,
                    Y2 = i * 30,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                PlacementField.Children.Add(verticalLine);
                PlacementField.Children.Add(horizontalLine);
            }

            // Добавляем координаты
            for (int i = 0; i < 10; i++)
            {
                // Буквы сверху
                var letterText = new TextBlock
                {
                    Text = ((char)('A' + i)).ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(letterText, i * 30 + 10);
                Canvas.SetTop(letterText, -20);
                PlacementField.Children.Add(letterText);

                // Цифры слева
                var numberText = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    FontSize = 12,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(numberText, -20);
                Canvas.SetTop(numberText, i * 30 + 10);
                PlacementField.Children.Add(numberText);
            }
        }

        private void Field_MouseClick(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PlacementField);
            int x = (int)(pos.X / 30);
            int y = (int)(pos.Y / 30);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
            {
                var vm = (VM.ShipPlacementVM)DataContext;
                if (vm.TryPlaceShip(x, y))
                {
                    DrawPlacedShips();
                }
            }
        }

        private void DrawPlacedShips()
        {
            // Очищаем старые корабли
            foreach (var preview in shipPreviews)
            {
                PlacementField.Children.Remove(preview);
            }
            shipPreviews.Clear();

            var vm = (VM.ShipPlacementVM)DataContext;
            var ships = vm.GetPlacedShips();
            var field = vm.GetGameField();

            // Рисуем все размещенные корабли
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (field[i, j] == 1)
                    {
                        var shipCell = new Rectangle
                        {
                            Width = 28,
                            Height = 28,
                            Fill = Brushes.DarkGray,
                            Stroke = Brushes.Black,
                            StrokeThickness = 1
                        };

                        Canvas.SetLeft(shipCell, i * 30 + 1);
                        Canvas.SetTop(shipCell, j * 30 + 1);

                        PlacementField.Children.Add(shipCell);
                        shipPreviews.Add(shipCell);
                    }
                }
            }
        }

        private void PlacementField_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(PlacementField);
            int x = (int)(pos.X / 30);
            int y = (int)(pos.Y / 30);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
            {
                // Можно добавить предпросмотр корабля при наведении
            }
        }

        private void ClearField_Click(object sender, RoutedEventArgs e)
        {
            DrawGrid();
        }
    }
}