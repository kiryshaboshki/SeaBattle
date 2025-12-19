using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeaBattle.View
{
    public partial class ShipPlacementPage : Page
    {
        public ShipPlacementPage()
        {
            InitializeComponent();
            DrawGrid();
        }

        private void DrawGrid()
        {
            PlacementField.Children.Clear();

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
        }

        private void Field_MouseClick(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(PlacementField);
            int x = (int)(pos.X / 30);
            int y = (int)(pos.Y / 30);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
            {
                var vm = (VM.ShipPlacementVM)DataContext;
                vm.SelectCell(x, y);
            }
        }
    }
}