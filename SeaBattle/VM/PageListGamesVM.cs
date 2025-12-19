// PageListGamesVM.cs (обновлённый)
using SeaBattleRepository.DTO;
using SeaBattleWPF.API;
using SeaBattleWPF.API.Game;
using SeaBattleWPF.mvvm;
using SeaBattleWPF.View;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace SeaBattleWPF.VM
{
    public class PageListGamesVM : BaseVM
    {
        private GameDTO selectedGame;
        private List<GameDTO> games;
        private TcpGameClient _client;

        public List<GameDTO> Games
        {
            get => games;
            set
            {
                games = value;
                Signal();
            }
        }

        public GameDTO SelectedGame
        {
            get => selectedGame;
            set
            {
                selectedGame = value;
                Signal();
            }
        }

        public CommandVM CreateGame { get; set; }
        public CommandVM JoinGame { get; set; }
        public CommandVM RefreshGames { get; set; }

        public PageListGamesVM()
        {
            _client = TcpGameClient.Instance;

            // Подписываемся на получение списка игр
            _client.OnGamesListReceived += HandleGamesListReceived;
            _client.OnGameUpdate += HandleGameCreated;
            _client.OnError += HandleError;

            // Запрашиваем список игр при запуске
            RequestGamesList();

            RefreshGames = new CommandVM(RequestGamesList);

            CreateGame = new CommandVM(async () =>
            {
                await _client.SendAsync("CREATE_GAME", new { });
            });

            JoinGame = new CommandVM(async () =>
            {
                if (SelectedGame != null)
                {
                    await _client.SendAsync("JOIN_GAME", new
                    {
                        GameId = SelectedGame.Id
                    });
                }
                else
                {
                    MessageBox.Show("Выберите игру для присоединения");
                }
            });
        }

        private void HandleGamesListReceived(List<GameDTO> gamesList)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Games = gamesList;
            });
        }

        private void HandleGameCreated(GameDTO game)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Если мы создали игру или присоединились
                if (game.IdUserNextTurn == 0) // Ожидаем соперника
                {
                    Game.CurrentGame = game;
                    Game.CreatorIsCurrentUser = true;
                    OpenGamePage();
                }
                else if (game.Opponent != null) // Присоединились к игре
                {
                    Game.CurrentGame = game;
                    Game.CreatorIsCurrentUser = false;
                    OpenGamePage();
                }
            });
        }

        private void HandleError(string error)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(error, "Ошибка");
            });
        }

        private void OpenGamePage()
        {
            // Переходим на страницу игры
            // Здесь нужно вызвать метод для открытия GamePage
            // В зависимости от вашей навигации
        }

        private void RequestGamesList()
        {
            Task.Run(async () =>
            {
                await _client.SendAsync("GET_GAMES", new { });
            });
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.OnGamesListReceived -= HandleGamesListReceived;
                _client.OnGameUpdate -= HandleGameCreated;
                _client.OnError -= HandleError;
            }
        }
    }
}