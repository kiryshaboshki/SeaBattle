using SeaBattleRepository.DTO;
using SeaBattleWPF.API;
using SeaBattleWPF.API.Game;
using SeaBattleWPF.VM;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeaBattleWPF.View
{
    public partial class ShipPlacementPage : Page
    {
        private ShipPlacementVM _vm;
        private bool _isGameAgainstAI;

        public ShipPlacementPage(bool isGameAgainstAI = false)
        {
            InitializeComponent();
            _isGameAgainstAI = isGameAgainstAI;
            _vm = new ShipPlacementVM();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _vm.RedrawField(PlacementCanvas);
            UpdateStatus();
        }

        private void PlacementCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _vm.PlaceShip(PlacementCanvas, e);
            UpdateStatus();
        }

        private void RotateButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.RotateShip();
            StatusText.Text = $"Корабль: {_vm.RotationText}";
        }

        private void RandomButton_Click(object sender, RoutedEventArgs e)
        {
            // Упрощённая случайная расстановка
            var randomField = GenerateRandomField();

            // Копируем в VM
            for (int i = 0; i < 100; i++)
            {
                // Здесь нужно заполнить поле VM
            }

            _vm.RedrawField(PlacementCanvas);
            StatusText.Text = "Корабли расставлены случайно";
            ReadyButton.IsEnabled = true;
        }

        private byte[] GenerateRandomField()
        {
            var field = new byte[100];
            var random = new Random();

            // Простая случайная расстановка для теста
            for (int i = 0; i < 20; i++) // 20 случайных клеток-кораблей
            {
                int x = random.Next(10);
                int y = random.Next(10);
                int index = x + y * 10;
                field[index] = 1;
            }

            return field;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _vm = new ShipPlacementVM();
            _vm.RedrawField(PlacementCanvas);
            UpdateStatus();
            ReadyButton.IsEnabled = false;
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_vm.AllShipsPlaced)
            {
                MessageBox.Show("Расставьте все корабли перед началом игры!");
                return;
            }

            var field = _vm.GetField();

            // Если игра против ИИ
            if (_isGameAgainstAI)
            {
                StartGameAgainstAI(field);
            }
            else
            {
                // Для онлайн игры - отправляем поле на сервер
                StartOnlineGame(field);
            }
        }

        private void StartGameAgainstAI(byte[] playerField)
        {
            try
            {
                var client = TcpGameClient.Instance;
                client.SetGameMode(GameMode.OfflineAI);

                // Создаём игру
                var game = new GameDTO
                {
                    Id = 1,
                    Creator = new UserDTO { Id = 1, Login = "Игрок", Rating = 1000 },
                    Opponent = new UserDTO { Id = 2, Login = "ИИ", Rating = 900 },
                    Status = 1,
                    IdUserNextTurn = 1,
                    DatetimeStartGame = DateTime.Now,
                    FieldUser1 = playerField // Сохраняем поле игрока
                };

                // Инициализируем глобальное состояние игры
                Game.CurrentGame = game;
                Game.CreatorIsCurrentUser = true;
                Game.SetState(States.MyTurn);

                // Переходим к игровой странице
                var gamePage = new GamePage();
                PageControl.GetInstance().CurrentPage = gamePage;

                // Регистрируем поле игрока
                var gameVM = (GameVM)gamePage.DataContext;
                gameVM.RegisterField(gamePage.FieldUser1, true);
                Game.RedrawMyField(gamePage.FieldUser1, playerField);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void StartOnlineGame(byte[] playerField)
        {
            // Для онлайн игры - здесь будет отправка на сервер
            MessageBox.Show("Онлайн игра пока не реализована");
        }

        private void UpdateStatus()
        {
            if (_vm.AllShipsPlaced)
            {
                StatusText.Text = "Все корабли расставлены! Нажмите 'Готово'.";
                ReadyButton.IsEnabled = true;
            }
            else
            {
                StatusText.Text = "Кликните на поле, чтобы разместить корабль";
                ReadyButton.IsEnabled = false;
            }
        }
    }
}