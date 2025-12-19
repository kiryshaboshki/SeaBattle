using SeaBattle.Models;
using SeaBattle.mvvm;
using SeaBattle.View;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using SeaBattle; // ДОБАВИТЬ USING

namespace SeaBattle.VM
{
    public class ShipPlacementVM : BaseVM
    {
        private int[,] gameField = new int[10, 10];
        private List<ShipPlacementData> placedShips = new List<ShipPlacementData>(); // ИЗМЕНИТЬ ТИП

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

            PlaceShipCommand = new CommandVM(() => { });

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

                if (!CurrentGame.IsOnline)
                {
                    MessageBox.Show("Начинаем оффлайн игру против компьютера!\n\n" +
                                   "Корабли расставлены. Удачи в битве!",
                        "Оффлайн режим", MessageBoxButton.OK, MessageBoxImage.Information);

                    SavePlacementForGame();
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new GamePage();
                    return;
                }

                StatusMessage = "Отправка расстановки на сервер...";

                try
                {
                    byte[] fieldData = ConvertFieldToByteArray();
                    var result = await API.GameAPI.SendFieldPlacement(fieldData);

                    if (result.Success)
                    {
                        MessageBox.Show("Расстановка принята сервером!\n\n" +
                                       "Ожидаем подключения противника...",
                            "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                        SavePlacementForGame();
                        var pageControl = PageControl.GetInstance();
                        pageControl.CurrentPage = new GamePage();
                    }
                    else
                    {
                        StatusMessage = $"Ошибка: {result.Message}";
                        MessageBox.Show($"Ошибка отправки расстановки:\n{result.Message}\n\n" +
                                       "Попробуйте еще раз или перейдите в оффлайн-режим.",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (System.Exception ex)
                {
                    StatusMessage = "Ошибка подключения к серверу";

                    var result = MessageBox.Show(
                        $"Не удалось подключиться к серверу:\n{ex.Message}\n\n" +
                        "Хотите продолжить в оффлайн-режиме против компьютера?",
                        "Ошибка сети", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        CurrentGame.IsOnline = false;
                        CurrentGame.OpponentName = "Компьютер";
                        SavePlacementForGame();
                        var pageControl = PageControl.GetInstance();
                        pageControl.CurrentPage = new GamePage();
                    }
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

            var ship = new ShipPlacementData // ИЗМЕНИТЬ НА ShipPlacementData
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

                gameField[cellX, cellY] = SelectedShip.Size;
                ship.Cells.Add((cellX, cellY));
            }

            placedShips.Add(ship);
            SelectedShip.Placed++;

            CheckAllShipsPlaced();
            StatusMessage = $"Корабль ({SelectedShip.Size}-палубный) размещен. Осталось разместить: {GetRemainingShips()}";
            return true;
        }

        private bool CanPlaceShip(int x, int y, int size, bool horizontal)
        {
            if (horizontal)
            {
                if (x + size > 10) return false;
            }
            else
            {
                if (y + size > 10) return false;
            }

            for (int i = 0; i < size; i++)
            {
                int cellX = horizontal ? x + i : x;
                int cellY = horizontal ? y : y + i;

                if (gameField[cellX, cellY] != 0) return false;

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
                            var ship = new ShipPlacementData // ИЗМЕНИТЬ
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

                                gameField[cellX, cellY] = shipModel.Size;
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
                StatusMessage = "✓ Все корабли размещены! Можно начинать игру.";
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

        private void SavePlacementForGame()
        {
            // Сохраняем в GameFieldData
            GameFieldData.PlayerField = gameField;
            GameFieldData.PlayerShips = placedShips.Select(s => new GameFieldShip
            {
                Size = s.Size,
                IsHorizontal = s.IsHorizontal,
                X = s.X,
                Y = s.Y,
                Cells = s.Cells
            }).ToList();
        }

        public List<ShipPlacementData> GetPlacedShips() => placedShips; // ИЗМЕНИТЬ ВОЗВРАЩАЕМЫЙ ТИП
        public int[,] GetGameField() => gameField;
    }

    // ПЕРЕИМЕНОВАНО из Ship
    public class ShipPlacementData
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