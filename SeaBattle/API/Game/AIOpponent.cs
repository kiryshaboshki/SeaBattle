using System;
using System.Collections.Generic;
using System.Linq;

namespace SeaBattleWPF.API
{
    public class AIOpponent
    {
        private byte[] _aiField = new byte[100];      // Поле ИИ (по нему стреляет игрок)
        private byte[] _playerField = new byte[100];  // Поле игрока (по нему стреляет ИИ)
        private List<int> _availableShots = new List<int>();
        private Random _random = new Random();
        private List<List<int>> _ships = new List<List<int>>();

        public AIOpponent()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Инициализируем список доступных выстрелов
            for (int i = 0; i < 100; i++)
                _availableShots.Add(i);

            // Расставляем корабли для ИИ
            PlaceShips();
        }

        private void PlaceShips()
        {
            // Очищаем поле
            for (int i = 0; i < 100; i++)
                _aiField[i] = 0;

            _ships.Clear();

            // Стандартный набор кораблей: 1x4, 2x3, 3x2, 4x1
            int[] ships = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };

            foreach (var shipSize in ships)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 1000)
                {
                    attempts++;

                    bool horizontal = _random.Next(2) == 0;
                    int x = _random.Next(10);
                    int y = _random.Next(10);

                    if (CanPlaceShip(x, y, shipSize, horizontal))
                    {
                        var shipCells = PlaceShip(x, y, shipSize, horizontal);
                        _ships.Add(shipCells);
                        placed = true;
                    }
                }

                if (!placed)
                {
                    // Если не удалось разместить, начинаем заново
                    Initialize();
                    return;
                }
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
                if (_aiField[index] != 0)
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
                            if (_aiField[nx + ny * 10] != 0)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private List<int> PlaceShip(int x, int y, int size, bool horizontal)
        {
            var shipCells = new List<int>();

            for (int i = 0; i < size; i++)
            {
                int posX = horizontal ? x + i : x;
                int posY = horizontal ? y : y + i;
                int index = posX + posY * 10;

                _aiField[index] = 1; // Корабль
                shipCells.Add(index);
            }

            return shipCells;
        }

        // Обработка выстрела игрока по полю ИИ
        public (bool hit, bool shipDestroyed, bool allShipsDestroyed) ProcessPlayerShot(int x, int y)
        {
            int index = x + y * 10;

            if (_aiField[index] == 1) // Попадание в корабль
            {
                _aiField[index] = 2; // Подбитая часть

                // Находим корабль
                var ship = _ships.FirstOrDefault(s => s.Contains(index));
                bool shipDestroyed = false;

                if (ship != null)
                {
                    // Проверяем, весь ли корабль подбит
                    shipDestroyed = ship.All(cell => _aiField[cell] == 2);

                    if (shipDestroyed)
                    {
                        // Помечаем клетки вокруг уничтоженного корабля
                        MarkAroundShip(ship);
                    }
                }

                bool allDestroyed = CheckAllShipsDestroyed();
                return (true, shipDestroyed, allDestroyed);
            }
            else if (_aiField[index] == 0) // Промах
            {
                _aiField[index] = 3; // Промах
                return (false, false, false);
            }

            return (false, false, false); // Уже стреляли сюда
        }

        private void MarkAroundShip(List<int> shipCells)
        {
            foreach (var cell in shipCells)
            {
                int x = cell % 10;
                int y = cell / 10;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;

                        if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                        {
                            int idx = nx + ny * 10;
                            if (_aiField[idx] == 0)
                            {
                                _aiField[idx] = 3; // Помечаем как промах вокруг корабля
                            }
                        }
                    }
                }
            }
        }

        // Ход ИИ
        public (int x, int y, bool hit, bool shipDestroyed) MakeTurn()
        {
            if (_availableShots.Count == 0)
                return (-1, -1, false, false);

            // Простая стратегия: случайный выстрел
            int shotIndex = _random.Next(_availableShots.Count);
            int cellIndex = _availableShots[shotIndex];
            _availableShots.RemoveAt(shotIndex);

            int x = cellIndex % 10;
            int y = cellIndex / 10;

            // Здесь можно добавить логику "стрельбы по игроку"
            // Но для простоты помечаем как попадание с вероятностью 30%
            bool hit = _random.Next(100) < 30;

            if (hit)
            {
                _playerField[cellIndex] = 2; // Попадание
            }
            else
            {
                _playerField[cellIndex] = 3; // Промах
            }

            return (x, y, hit, false);
        }

        private bool CheckAllShipsDestroyed()
        {
            // Проверяем, остались ли неподбитые корабли
            return !_aiField.Any(cell => cell == 1);
        }

        public byte[] GetAIField()
        {
            return _aiField;
        }

        public byte[] GetPlayerField()
        {
            return _playerField;
        }

        // Для тестирования
        public void SetTestField(byte[] field)
        {
            Array.Copy(field, _aiField, 100);
        }
    }
}