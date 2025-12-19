using SeaBattleRepository.DTO;
using SeaBattleWPF.API.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SeaBattleWPF.API
{
    public class TcpGameClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private bool _isConnected = false;
        private GameMode _currentMode = GameMode.Online;
        private AIOpponent _aiOpponent;

        // События
        public event Action<GameTurn> OnTurnReceived;
        public event Action<GameDTO> OnGameUpdate;
        public event Action<List<GameDTO>> OnGamesListReceived;
        public event Action<string> OnError;
        public event Action OnDisconnected;
        public event Action<UserDTO> OnAuthSuccess;

        private static TcpGameClient _instance;
        public static TcpGameClient Instance => _instance ??= new TcpGameClient();

        private TcpGameClient() { }

        public void SetGameMode(GameMode mode)
        {
            _currentMode = mode;

            if (mode == GameMode.OfflineAI)
            {
                _aiOpponent = new AIOpponent();
                _isConnected = true; // Важно: в оффлайн режиме мы "подключены"
            }
            else if (mode == GameMode.Test)
            {
                _isConnected = true; // Тестовый режим тоже "подключен"
            }
            else
            {
                _isConnected = false; // Онлайн режим - нужно реальное подключение
            }
        }

        public async Task<bool> ConnectAsync(string ip, int port)
        {
            if (_currentMode != GameMode.Online)
            {
                // В оффлайн режиме сразу "подключаемся"
                _isConnected = true;
                return true;
            }

            try
            {
                Disconnect();

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(ip, port);
                _stream = _tcpClient.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                _isConnected = true;

                // Запускаем фоновую задачу для чтения сообщений
                _ = Task.Run(ListenForMessages);

                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        private async Task ListenForMessages()
        {
            while (_isConnected && _tcpClient.Connected)
            {
                try
                {
                    var json = await _reader.ReadLineAsync();
                    if (string.IsNullOrEmpty(json))
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    ProcessMessage(json);
                }
                catch (Exception ex)
                {
                    if (_isConnected)
                    {
                        OnError?.Invoke($"Ошибка соединения: {ex.Message}");
                        Disconnect();
                    }
                    break;
                }
            }
        }

        private void ProcessMessage(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                var command = root.GetProperty("Command").GetString();
                var data = root.GetProperty("Data");

                switch (command)
                {
                    case "TURN":
                        var turn = JsonSerializer.Deserialize<GameTurn>(data.GetRawText());
                        Application.Current.Dispatcher.Invoke(() =>
                            OnTurnReceived?.Invoke(turn));
                        break;

                    case "GAME_UPDATE":
                        var game = JsonSerializer.Deserialize<GameDTO>(data.GetRawText());
                        Application.Current.Dispatcher.Invoke(() =>
                            OnGameUpdate?.Invoke(game));
                        break;

                    case "GAMES_LIST":
                        var games = JsonSerializer.Deserialize<List<GameDTO>>(data.GetRawText());
                        Application.Current.Dispatcher.Invoke(() =>
                            OnGamesListReceived?.Invoke(games));
                        break;

                    case "AUTH_SUCCESS":
                        var user = JsonSerializer.Deserialize<UserDTO>(data.GetRawText());
                        Application.Current.Dispatcher.Invoke(() =>
                            OnAuthSuccess?.Invoke(user));
                        break;

                    case "ERROR":
                        var errorMsg = data.GetString();
                        Application.Current.Dispatcher.Invoke(() =>
                            OnError?.Invoke(errorMsg));
                        break;
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    OnError?.Invoke($"Ошибка обработки сообщения: {ex.Message}"));
            }
        }

        public async Task SendAsync<T>(string command, T data)
        {
            if (_currentMode == GameMode.OfflineAI)
            {
                // Обрабатываем команды локально
                ProcessOfflineCommand(command, data);
                return;
            }
            else if (_currentMode == GameMode.Test)
            {
                // Тестовый режим - игнорируем
                return;
            }

            if (!_isConnected || !_tcpClient.Connected)
            {
                OnError?.Invoke("Нет подключения к серверу");
                return;
            }

            try
            {
                var message = new
                {
                    Command = command,
                    Data = data
                };

                var json = JsonSerializer.Serialize(message);
                await _writer.WriteLineAsync(json);
            }
            catch (Exception ex)
            {
                OnError?.Invoke($"Ошибка отправки: {ex.Message}");
                Disconnect();
            }
        }

        private void ProcessOfflineCommand<T>(string command, T data)
        {
            try
            {
                switch (command)
                {
                    case "AUTH":
                        var authData = JsonSerializer.Deserialize<AuthRequest>(
                            JsonSerializer.Serialize(data));

                        var user = new UserDTO
                        {
                            Id = 1,
                            Login = authData.Login,
                            Password = authData.Password,
                            Rating = 1000
                        };

                        Application.Current.Dispatcher.Invoke(() =>
                            OnAuthSuccess?.Invoke(user));
                        break;

                    case "REGISTER":
                        var regData = JsonSerializer.Deserialize<AuthRequest>(
                            JsonSerializer.Serialize(data));

                        // В оффлайн режиме регистрация всегда успешна
                        Application.Current.Dispatcher.Invoke(() =>
                            OnAuthSuccess?.Invoke(new UserDTO
                            {
                                Id = 1,
                                Login = regData.Login,
                                Rating = 1000
                            }));
                        break;

                    case "CREATE_GAME":
                        var game = new GameDTO
                        {
                            Id = 1,
                            Creator = new UserDTO { Id = 1, Login = "Игрок", Rating = 1000 },
                            Opponent = new UserDTO { Id = 2, Login = "ИИ", Rating = 900 },
                            Status = 1,
                            IdUserNextTurn = 1,
                            DatetimeStartGame = DateTime.Now
                        };

                        Application.Current.Dispatcher.Invoke(() =>
                            OnGameUpdate?.Invoke(game));
                        break;

                    case "GET_GAMES":
                        var gamesList = new List<GameDTO>
                        {
                            new GameDTO
                            {
                                Id = 1,
                                Creator = new UserDTO { Id = 1, Login = "Оффлайн игра", Rating = 1000 },
                                Status = 0
                            }
                        };

                        Application.Current.Dispatcher.Invoke(() =>
                            OnGamesListReceived?.Invoke(gamesList));
                        break;

                    case "JOIN_GAME":
                        var joinData = JsonSerializer.Deserialize<JoinGameRequest>(
                            JsonSerializer.Serialize(data));

                        var joinedGame = new GameDTO
                        {
                            Id = joinData.GameId,
                            Creator = new UserDTO { Id = 2, Login = "ИИ", Rating = 900 },
                            Opponent = new UserDTO { Id = 1, Login = "Игрок", Rating = 1000 },
                            Status = 1,
                            IdUserNextTurn = 2, // Первым ходит ИИ
                            DatetimeStartGame = DateTime.Now
                        };

                        Application.Current.Dispatcher.Invoke(() =>
                            OnGameUpdate?.Invoke(joinedGame));

                        // ИИ делает первый ход через 1 секунду
                        Task.Delay(1000).ContinueWith(_ =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                MakeAITurn());
                        });
                        break;

                    case "MAKE_TURN":
                        var turnData = JsonSerializer.Deserialize<MakeTurnRequest>(
                            JsonSerializer.Serialize(data));

                        // Обрабатываем выстрел игрока
                        var (hit, shipDestroyed, allDestroyed) = _aiOpponent.ProcessPlayerShot(
                            turnData.X, turnData.Y);

                        // Обновляем поле игрока
                        var playerField = _aiOpponent.GetAIField();
                        var turn = new GameTurn
                        {
                            IdUserNextTurn = hit ? 1 : 2, // Если попал - ходит снова
                            FieldUser = playerField
                        };

                        if (allDestroyed)
                        {
                            turn.IdWinner = 1; // Игрок победил
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                            OnTurnReceived?.Invoke(turn));

                        // Если промахнулся - ходит ИИ через 1 секунду
                        if (!hit && !allDestroyed)
                        {
                            Task.Delay(1000).ContinueWith(_ =>
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                    MakeAITurn());
                            });
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                    OnError?.Invoke($"Оффлайн ошибка: {ex.Message}"));
            }
        }

        private void MakeAITurn()
        {
            if (_aiOpponent == null) return;

            var (x, y, hit, shipDestroyed) = _aiOpponent.MakeTurn();

            // Обновляем поле игрока
            var aiField = _aiOpponent.GetPlayerField();
            var turn = new GameTurn
            {
                IdUserNextTurn = 1, // Следующий ход игрока
                FieldUser = aiField
            };

            Application.Current.Dispatcher.Invoke(() =>
                OnTurnReceived?.Invoke(turn));

            // Если ИИ попал, он ходит снова через 1 секунду
            if (hit)
            {
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                        MakeAITurn());
                });
            }
        }

        public void Disconnect()
        {
            _isConnected = false;
            try
            {
                _reader?.Close();
                _writer?.Close();
                _stream?.Close();
                _tcpClient?.Close();
            }
            catch { }
            OnDisconnected?.Invoke();
        }

        public bool IsConnected => _isConnected;

        // Оффлайн методы для тестирования
        public void SetTestField(byte[] field)
        {
            if (_aiOpponent != null)
            {
                _aiOpponent.SetTestField(field);
            }
        }

        public byte[] GetTestField()
        {
            return _aiOpponent?.GetAIField() ?? new byte[100];
        }
    }

}