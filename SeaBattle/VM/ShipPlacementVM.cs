using SeaBattle.Models;
using SeaBattle.mvvm;
using SeaBattle.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattle.VM
{
    public class ShipPlacementVM : BaseVM
    {
        private int[,] gameField = new int[10, 10]; // 0 - пусто, 1 - корабль
        private List<Ship> placedShips = new List<Ship>();

        public ObservableCollection<ShipModel> AvailableShips { get; set; }

        private ShipModel selectedShip;
        public ShipModel SelectedShip
        {
            get => selectedShip;
            set
            {
                selectedShip = value;
                Signal();
            }
        }

        private bool isHorizontal = true;
        public bool IsHorizontal
        {
            get => isHorizontal;
            set { isHorizontal = value; Signal(); }
        }

        private string statusMessage = "Выберите корабль и разместите его на поле";
        public string StatusMessage
        {
            get => statusMessage;
            set { statusMessage = value; Signal(); }
        }

        private bool canStartGame = false;
        public bool CanStartGame
        {
            get => canStartGame;
            set { canStartGame = value; Signal(); }
        }

        public CommandVM PlaceShipCommand { get; set; }
        public CommandVM ClearFieldCommand { get; set; }
        public CommandVM StartGameCommand { get; set; }
        public CommandVM RandomPlacementCommand { get; set; }
        public CommandVM RotateCommand { get; set; }

        public ShipPlacementVM()
        {
            AvailableShips = new ObservableCollection<ShipModel>
            {
                new ShipModel { Size = 4, Count = 1, Placed = 0, Description = "4-палубный (1 шт)" },
                new ShipModel { Size = 3, Count = 2, Placed = 0, Description = "3-палубный (2 шт)" },
                new ShipModel { Size = 2, Count = 3, Placed = 0, Description = "2-палубный (3 шт)" },
                new ShipModel { Size = 1, Count = 4, Placed = 0, Description = "1-палубный (4 шт)" }
            };

            SelectedShip = AvailableShips[0];

            PlaceShipCommand = new CommandVM(() =>
            {
                // Команда будет вызываться при клике на поле
            });

            ClearFieldCommand = new CommandVM(() =>
            {
                ClearField();
                StatusMessage = "Поле очищено. Разместите корабли заново.";
            });

            StartGameCommand = new CommandVM(async () =>
            {
                if (!CanStartGame)
                {
                    MessageBox.Show("Разместите все корабли перед началом игры!",
                        "Не все корабли размещены", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Отправляем расстановку на сервер
                StatusMessage = "Отправка расстановки на сервер...";

                try
                {
                    // Преобразуем поле в массив для сервера
                    byte[] fieldData = ConvertFieldToByteArray();

                    // Отправляем на сервер
                    var result = await API.GameAPI.SendFieldPlacement(fieldData);

                    if (result.Success)
                    {
                        MessageBox.Show("Расстановка принята! Начинаем игру...",
                            "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Переход на игровую страницу
                        var pageControl = PageControl.GetInstance();
                        pageControl.CurrentPage = new GamePage();
                    }
                    else
                    {
                        StatusMessage = $"Ошибка: {result.Message}";
                        MessageBox.Show($"Ошибка отправки расстановки: {result.Message}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "Ошибка подключения к серверу";
                    MessageBox.Show($"Ошибка: {ex.Message}\nИгра начнется в оффлайн-режиме.",
                        "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Warning);

                    // Переход в оффлайн-режим
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new GamePage();
                }
            });

            RandomPlacementCommand = new CommandVM(() =>
            {
                ClearField();
                RandomPlaceShips();
                StatusMessage = "Корабли размещены случайным образом. Проверьте расстановку.";
            });

            RotateCommand = new CommandVM(() =>
            {
                IsHorizontal = !IsHorizontal;
                StatusMessage = IsHorizontal ? "Ориентация: Горизонтально" : "Ориентация: Вертикально";
            });

            InitializeField();
        }

        private void InitializeField()
        {
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                    gameField[i, j] = 0;
        }

        public bool TryPlaceShip(int x, int y)
        {
            if (SelectedShip == null || SelectedShip.Placed >= SelectedShip.Count)
            {
                StatusMessage = "Выбраны все корабли этого типа";
                return false;
            }

            if (!CanPlaceShip(x, y, SelectedShip.Size, IsHorizontal))
            {
                StatusMessage = "Невозможно разместить корабль здесь";
                return false;
            }

            // Размещаем корабль
            var ship = new Ship
            {
                Size = SelectedShip.Size,
                IsHorizontal = IsHorizontal,
                X = x,
                Y = y,
                Cells = new List<(int, int)>()
            };

            for (int i = 0; i < SelectedShip.Size; i++)
            {
                int cellX = IsHorizontal ? x + i : x;
                int cellY = IsHorizontal ? y : y + i;

                gameField[cellX, cellY] = 1;
                ship.Cells.Add((cellX, cellY));
            }

            placedShips.Add(ship);
            SelectedShip.Placed++;

            // Проверяем, все ли корабли размещены
            CheckAllShipsPlaced();

            StatusMessage = $"Корабль ({SelectedShip.Size}-палубный) размещен. Осталось разместить: {GetRemainingShips()}";
            return true;
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            // Проверяем границы поля
            if (horizontal)
            {
                if (x + size > 10) return false;
            }
            else
            {
                if (y + size > 10) return false;
            }

            // Проверяем клетки и соседние
            for (int i = 0; i < size; i++)
            {
                int cellX = horizontal ? x + i : x;
                int cellY = horizontal ? y : y + i;

                // Проверяем саму клетку
                if (gameField[cellX, cellY] != 0) return false;

                // Проверяем соседние клетки (включая диагонали)
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = cellX + dx;
                        int ny = cellY + dy;

                        if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                        {
                            if (gameField[nx, ny] != 0) return false;
                        }
                    }
                }
            }

            return true;
        }

        private void ClearField()
        {
            InitializeField();
            placedShips.Clear();

            foreach (var ship in AvailableShips)
            {
                ship.Placed = 0;
            }

            CanStartGame = false;
            Signal(nameof(AvailableShips));
        }

        private void RandomPlaceShips()
        {
            ClearField();
            var random = new System.Random();

            foreach (var shipModel in AvailableShips.OrderByDescending(s => s.Size))
            {
                for (int i = 0; i < shipModel.Count; i++)
                {
                    bool placed = false;
                    int attempts = 0;

                    while (!placed && attempts < 100)
                    {
                        int x = random.Next(0, 10);
                        int y = random.Next(0, 10);
                        bool horizontal = random.Next(0, 2) == 0;

                        if (CanPlaceShip(x, y, shipModel.Size, horizontal))
                        {
                            // Размещаем корабль
                            var ship = new Ship
                            {
                                Size = shipModel.Size,
                                IsHorizontal = horizontal,
                                X = x,
                                Y = y,
                                Cells = new List<(int, int)>()
                            };

                            for (int j = 0; j < shipModel.Size; j++)
                            {
                                int cellX = horizontal ? x + j : x;
                                int cellY = horizontal ? y : y + j;

                                gameField[cellX, cellY] = 1;
                                ship.Cells.Add((cellX, cellY));
                            }

                            placedShips.Add(ship);
                            shipModel.Placed++;
                            placed = true;
                        }
                        attempts++;
                    }
                }
            }

            CheckAllShipsPlaced();
            StatusMessage = "Все корабли размещены случайным образом";
        }

        private void CheckAllShipsPlaced()
        {
            CanStartGame = AvailableShips.All(s => s.Placed == s.Count);
            if (CanStartGame)
            {
                StatusMessage = "Все корабли размещены! Можно начинать игру.";
            }
        }

        private string GetRemainingShips()
        {
            var remaining = new List<string>();
            foreach (var ship in AvailableShips)
            {
                if (ship.Placed < ship.Count)
                {
                    remaining.Add($"{ship.Size}-палубных: {ship.Count - ship.Placed}");
                }
            }
            return string.Join(", ", remaining);
        }

        private byte[] ConvertFieldToByteArray()
        {
            // Преобразуем поле в массив байт для отправки на сервер
            // Каждый байт: 0 - пусто, 1 - корабль
            byte[] result = new byte[100];

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    result[i * 10 + j] = (byte)gameField[i, j];
                }
            }

            return result;
        }

        public List<Ship> GetPlacedShips() => placedShips;
        public int[,] GetGameField() => gameField;
    }

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

    public class ShipModel
    {
        public int Size { get; set; }
        public int Count { get; set; }
        public int Placed { get; set; }
        public string Description { get; set; }
    }
}