using SeaBattle.Models;
using SeaBattle.mvvm;
using SeaBattle.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace SeaBattle.VM
{
    public class ShipPlacementVM : BaseVM
    {
        public ObservableCollection<ShipModel> AvailableShips { get; set; }

        private bool isHorizontal = true;
        public bool IsHorizontal
        {
            get => isHorizontal;
            set { isHorizontal = value; Signal(); }
        }

        public CommandVM PlaceShipCommand { get; set; }
        public CommandVM ClearFieldCommand { get; set; }
        public CommandVM StartGameCommand { get; set; }

        public ShipPlacementVM()
        {
            AvailableShips = new ObservableCollection<ShipModel>
            {
                new ShipModel { Size = 4, Count = 1, Description = "4-палубный (1 шт)" },
                new ShipModel { Size = 3, Count = 2, Description = "3-палубный (2 шт)" },
                new ShipModel { Size = 2, Count = 3, Description = "2-палубный (3 шт)" },
                new ShipModel { Size = 1, Count = 4, Description = "1-палубный (4 шт)" }
            };

            PlaceShipCommand = new CommandVM(() =>
            {
                MessageBox.Show("Корабль размещен (заглушка)");
                // Здесь будет логика размещения
            });

            ClearFieldCommand = new CommandVM(() =>
            {
                MessageBox.Show("Поле очищено");
            });

            StartGameCommand = new CommandVM(() =>
            {
                MessageBox.Show("Начинаем игру!");
                var pageControl = PageControl.GetInstance();
                pageControl.CurrentPage = new GamePage();
            });
        }

        public void SelectCell(int x, int y)
        {
            // Логика выбора клетки для размещения
        }
    }

    public class ShipModel
    {
        public int Size { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }
    }
}