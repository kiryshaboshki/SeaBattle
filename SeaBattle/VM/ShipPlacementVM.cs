// ShipPlacementVM.cs - новая ViewModel для расстановки кораблей
using SeaBattleWPF.mvvm;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SeaBattleWPF.VM
{
    public class ShipPlacementVM : BaseVM
    {
        private byte[] _field = new byte[100];
        private List<Ship> _ships = new List<Ship>();
        private Ship _currentShip;
        private bool _isHorizontal = true;

        public byte[] Field => _field;

        public ShipPlacementVM()
        {
            InitializeShips();
        }

        private void InitializeShips()
        {
            // Стандартный набор кораблей
            _ships = new List<Ship>
            {
                new Ship(4), // 1 корабль на 4 клетки
                new Ship(3), new Ship(3), // 2 корабля на 3 клетки
                new Ship(2), new Ship(2), new Ship(2), // 3 корабля на 2 клетки
                new Ship(1), new Ship(1), new Ship(1), new Ship(1) // 4 корабля на 1 клетку
            };

            _currentShip = _ships[0];
        }

        public void RotateShip()
        {
            _isHorizontal = !_isHorizontal;
            Signal(nameof(RotationText));
        }

        public string RotationText => _isHorizontal ? "Горизонтально" : "Вертикально";

        public void PlaceShip(Canvas canvas, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(canvas);
            int x = (int)Math.Round(position.X) / 30;
            int y = (int)Math.Round(position.Y) / 30;

            if (CanPlaceShip(x, y, _currentShip.Size, _isHorizontal))
            {
                // Размещаем корабль
                for (int i = 0; i < _currentShip.Size; i++)
                {
                    int posX = _isHorizontal ? x + i : x;
                    int posY = _isHorizontal ? y : y + i;
                    int index = posX + posY * 10;
                    _field[index] = 1;
                }

                // Помечаем корабль как размещённый
                _currentShip.IsPlaced = true;

                // Переходим к следующему кораблю
                var nextShip = _ships.Find(s => !s.IsPlaced);
                if (nextShip != null)
                {
                    _currentShip = nextShip;
                }
                else
                {
                    // Все корабли расставлены
                    MessageBox.Show("Все корабли расставлены! Нажмите 'Готово' для начала игры.");
                }

                // Перерисовываем поле
                RedrawField(canvas);
            }
            else
            {
                MessageBox.Show("Нельзя разместить корабль здесь!");
            }
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int posX = horizontal ? x + i : x;
                int posY = horizontal ? y : y + i;

                if (posX >= 10 || posY >= 10)
                    return false;

                int index = posX + posY * 10;

                // Проверяем саму клетку
                if (_field[index] != 0)
                    return false;

                // Проверяем соседние клетки
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = posX + dx;
                        int ny = posY + dy;

                        if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                        {
                            if (_field[nx + ny * 10] != 0)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        public void RedrawField(Canvas canvas)
        {
            canvas.Children.Clear();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    int index = i + j * 10;
                    Rectangle box = new Rectangle
                    {
                        Width = 30,
                        Height = 30,
                        Fill = GetCellBrush(_field[index])
                    };

                    Canvas.SetLeft(box, i * 30);
                    Canvas.SetTop(box, j * 30);
                    canvas.Children.Add(box);
                }
            }
        }

        private Brush GetCellBrush(byte cellValue)
        {
            return cellValue switch
            {
                1 => Brushes.Gray, // Корабль
                _ => Brushes.LightBlue // Море
            };
        }

        public byte[] GetField()
        {
            return _field;
        }

        public bool AllShipsPlaced => _ships.TrueForAll(s => s.IsPlaced);

        private class Ship
        {
            public int Size { get; }
            public bool IsPlaced { get; set; }

            public Ship(int size)
            {
                Size = size;
                IsPlaced = false;
            }
        }
    }
}