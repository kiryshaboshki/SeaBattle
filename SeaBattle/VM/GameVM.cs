using SeaBattleRepository.DTO;
using SeaBattleWPF.API;
using SeaBattleWPF.API.Game;
using SeaBattleWPF.mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SeaBattleWPF.VM
{
    public class GameVM : BaseVM
    {
        private string _message;
        private TcpGameClient _client;
        private Dispatcher _dispatcher;
        private byte[] _playerField;
        private byte[] _opponentField = new byte[100]; // Поле противника (для отслеживания выстрелов)
        private Random _random = new Random();

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                Signal();
            }
        }

        public GameVM()
        {
            _client = TcpGameClient.Instance;
            _playerField = new byte[100];

            // Подписываемся на события
            _client.OnTurnReceived += HandleTurnReceived;
            _client.OnGameUpdate += HandleGameUpdate;
            _client.OnError += HandleError;

            InitializeGameState();
        }

        private void InitializeGameState()
        {
            if (Game.CurrentGame == null)
            {
                Game.CurrentGame = new GameDTO
                {
                    Id = 1,
                    Creator = new UserDTO { Id = 1, Login = "Игрок", Rating = 1000 },
                    Opponent = new UserDTO { Id = 2, Login = "Противник", Rating = 900 },
                    Status = 1,
                    IdUserNextTurn = 1,
                    DatetimeStartGame = DateTime.Now
                };
            }

            if (Game.CreatorIsCurrentUser)
            {
                Game.SetState(States.WaitJoin);
                Message = "Ожидаем присоединения противника...";
            }
            else
            {
                Game.SetState(States.WaitTurn);
                Message = "Ожидаем ход противника...";
            }
        }

        private void HandleTurnReceived(GameTurn turn)
        {
            _dispatcher?.Invoke(() =>
            {
                if (turn.FieldUser != null && turn.FieldUser.Length == 100)
                {
                    _playerField = turn.FieldUser;

                    if (_fieldUser1Canvas != null)
                    {
                        Game.RedrawMyField(_fieldUser1Canvas, _playerField);
                    }
                }

                Game.TestTurn(turn);

                if (turn.IdWinner > 0)
                {
                    bool isWinner = turn.IdWinner == Game.CurrentGame?.Creator?.Id;
                    string message = isWinner ?
                        "🎉 Поздравляем! Вы победили!" :
                        "😔 Вы проиграли. Попробуйте снова!";

                    MessageBox.Show(message, "Игра окончена",
                                  MessageBoxButton.OK,
                                  isWinner ? MessageBoxImage.Information : MessageBoxImage.Exclamation);
                }
                else
                {
                    if (turn.IdUserNextTurn == Game.CurrentGame?.Creator?.Id)
                    {
                        Message = "Ваш ход!";
                        Game.SetState(States.MyTurn);
                    }
                    else
                    {
                        Message = "Ход противника...";
                        Game.SetState(States.WaitTurn);
                    }
                }
            });
        }

        private void HandleGameUpdate(GameDTO game)
        {
            _dispatcher?.Invoke(() =>
            {
                Game.CurrentGame = game;

                if (game.Status == 0)
                {
                    Message = "Ожидаем второго игрока...";
                    Game.SetState(States.WaitJoin);
                }
                else if (game.Status == 1)
                {
                    if (game.IdUserNextTurn == Game.CurrentGame?.Creator?.Id)
                    {
                        Message = "Ваш ход!";
                        Game.SetState(States.MyTurn);
                    }
                    else
                    {
                        Message = "Ход противника...";
                        Game.SetState(States.WaitTurn);
                    }
                }
                else if (game.Status == 2)
                {
                    Message = "Игра завершена";
                }

                Signal(nameof(Message));
            });
        }

        private void HandleError(string error)
        {
            _dispatcher?.Invoke(() =>
            {
                MessageBox.Show(error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Message = $"Ошибка: {error}";
            });
        }

        internal void ClickField(Canvas fieldUser, MouseButtonEventArgs e)
        {
            if (fieldUser == _fieldUser2Canvas &&
                Game.CurrentGame?.IdUserNextTurn == Game.CurrentGame?.Creator?.Id)
            {
                var position = e.GetPosition(fieldUser);
                int x = (int)Math.Round(position.X) / 30;
                int y = (int)Math.Round(position.Y) / 30;

                if (x >= 0 && x < 10 && y >= 0 && y < 10)
                {
                    // Исправленная проверка
                    if (!IsCellAlreadyShot(x, y))
                    {
                        MakeTurn(x, y);
                    }
                    else
                    {
                        MessageBox.Show("Вы уже стреляли в эту клетку!",
                                      "Повторный выстрел",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                    }
                }
            }
            else if (fieldUser == _fieldUser2Canvas)
            {
                MessageBox.Show("Сейчас не ваш ход!",
                              "Ожидайте",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        // НОВЫЙ МЕТОД: Проверка, стреляли ли уже в клетку
        private bool IsCellAlreadyShot(int x, int y)
        {
            int index = x + y * 10;
            // Проверяем по массиву opponentField
            // 0 = неизвестно, 2 = попадание, 3 = промах
            return _opponentField[index] == 2 || _opponentField[index] == 3;
        }

        private async void MakeTurn(int x, int y)
        {
            if (Game.CurrentGame == null)
            {
                MessageBox.Show("Игра не инициализирована", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int index = x + y * 10;

            // Сразу отмечаем в массиве, что стреляли (ожидаем результат)
            _opponentField[index] = 4; // 4 = ожидание результата

            // Отображаем выстрел (серый кружок - ожидание)
            DrawShotMarker(_fieldUser2Canvas, x, y, Brushes.Gray);

            // Отправляем ход
            await _client.SendAsync("MAKE_TURN", new
            {
                GameId = Game.CurrentGame.Id,
                X = x,
                Y = y
            });

            Message = "Ожидаем результат выстрела...";
        }

        private void DrawShotMarker(Canvas canvas, int x, int y, Brush color)
        {
            // Очищаем старые маркеры в этой клетке
            ClearCellMarkers(canvas, x, y);

            // Рисуем новый маркер
            Ellipse marker = new Ellipse
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

        private void ClearCellMarkers(Canvas canvas, int x, int y)
        {
            // Удаляем все элементы в этой клетке
            List<UIElement> toRemove = new List<UIElement>();

            foreach (var child in canvas.Children)
            {
                if (child is Shape shape)
                {
                    var left = Canvas.GetLeft(shape);
                    var top = Canvas.GetTop(shape);

                    int cellX = (int)(left / 30);
                    int cellY = (int)(top / 30);

                    if (cellX == x && cellY == y)
                    {
                        toRemove.Add(shape);
                    }
                }
            }

            foreach (var element in toRemove)
            {
                canvas.Children.Remove(element);
            }
        }

        private Canvas _fieldUser1Canvas;
        private Canvas _fieldUser2Canvas;

        internal void RegisterField(Canvas fieldUser, bool currentUser)
        {
            if (currentUser)
            {
                _fieldUser1Canvas = fieldUser;
                if (_playerField != null && _playerField.Length == 100)
                {
                    Game.RedrawMyField(_fieldUser1Canvas, _playerField);
                }
            }
            else
            {
                _fieldUser2Canvas = fieldUser;
            }

            Game.RegisterField(fieldUser, currentUser);
        }

        public void SetPlayerField(byte[] field)
        {
            if (field != null && field.Length == 100)
            {
                _playerField = field;

                if (Game.CurrentGame != null)
                {
                    Game.CurrentGame.FieldUser1 = field;
                }

                if (_fieldUser1Canvas != null)
                {
                    Game.RedrawMyField(_fieldUser1Canvas, _playerField);
                }
            }
        }

        // Старый метод (оставляем для совместимости)
        public byte[] GenerateRandomField()
        {
            return GenerateValidField();
        }

        // Новый правильный метод
        public byte[] GenerateValidField()
        {
            var field = new byte[100];

            // Список кораблей для расстановки
            var ships = new List<(int size, int count)>
            {
                (4, 1),  // 1 корабль на 4 клетки
                (3, 2),  // 2 корабля на 3 клетки
                (2, 3),  // 3 корабля на 2 клетки
                (1, 4)   // 4 корабля на 1 клетку
            };

            foreach (var (size, count) in ships)
            {
                for (int shipNum = 0; shipNum < count; shipNum++)
                {
                    bool placed = false;
                    int attempts = 0;

                    while (!placed && attempts < 1000)
                    {
                        attempts++;

                        int x = _random.Next(10);
                        int y = _random.Next(10);
                        bool horizontal = _random.Next(2) == 0;

                        if (CanPlaceShip(field, x, y, size, horizontal))
                        {
                            PlaceShip(field, x, y, size, horizontal);
                            placed = true;
                        }
                    }

                    if (!placed)
                    {
                        // Начинаем заново
                        return GenerateValidField();
                    }
                }
            }

            return field;
        }

        private bool CanPlaceShip(byte[] field, int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int posX = horizontal ? x + i : x;
                int posY = horizontal ? y : y + i;

                if (posX >= 10 || posY >= 10)
                    return false;

                int index = posX + posY * 10;

                if (field[index] != 0)
                    return false;

                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int nx = posX + dx;
                        int ny = posY + dy;

                        if (nx >= 0 && nx < 10 && ny >= 0 && ny < 10)
                        {
                            if (field[nx + ny * 10] != 0)
                                return false;
                        }
                    }
                }
            }

            return true;
        }

        private void PlaceShip(byte[] field, int x, int y, int size, bool horizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int posX = horizontal ? x + i : x;
                int posY = horizontal ? y : y + i;
                int index = posX + posY * 10;
                field[index] = 1;
            }
        }

        internal void RegisterDispatcher(Dispatcher dispatcher)
        {
            this._dispatcher = dispatcher;
        }

        public async Task SendReadyAsync(byte[] field)
        {
            if (Game.CurrentGame == null) return;

            SetPlayerField(field);

            await _client.SendAsync("READY_TO_PLAY", new
            {
                GameId = Game.CurrentGame.Id,
                Field = field
            });
        }

        public void Dispose()
        {
            try
            {
                if (_client != null)
                {
                    _client.OnTurnReceived -= HandleTurnReceived;
                    _client.OnGameUpdate -= HandleGameUpdate;
                    _client.OnError -= HandleError;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing GameVM: {ex.Message}");
            }
        }
    }
}