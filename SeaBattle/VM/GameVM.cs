using SeaBattle.mvvm;
using System;

namespace SeaBattle.VM
{
    public class GameVM : BaseVM
    {
        private string gameStatus = "Ожидание хода";
        public string GameStatus
        {
            get => gameStatus;
            set { gameStatus = value; Signal(); }
        }

        private string currentPlayer = "Вы";
        public string CurrentPlayer
        {
            get => currentPlayer;
            set { currentPlayer = value; Signal(); }
        }

        public CommandVM ExitCommand { get; set; }

        public GameVM()
        {
            ExitCommand = new CommandVM(() =>
            {
                // Здесь будет выход из игры
                var pageControl = PageControl.GetInstance();
                pageControl.CurrentPage = new View.PageListGames();
            });
        }
    }
}