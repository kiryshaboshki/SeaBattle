using SeaBattle.API;
using SeaBattle.mvvm;
using SeaBattle.View;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace SeaBattle.VM
{
    public class PageListGamesVM : BaseVM
    {
        public CommandVM CreateGameCommand { get; set; }
        public CommandVM RefreshCommand { get; set; }

        public PageListGamesVM()
        {
            CreateGameCommand = new CommandVM(async () =>
            {
                var result = await Client.Instance.PostAsync("GameInfo/CreateGame");

                if (result.Success)
                {
                    MessageBox.Show("Игра создана! Ожидаем соперника...");
                    // Здесь позже будет переход на игровую страницу
                }
                else
                {
                    MessageBox.Show($"Ошибка создания игры: {result.Response}");
                }
            });

            RefreshCommand = new CommandVM(async () =>
            {
                MessageBox.Show("Обновляем список игр...");
                // Здесь позже будет загрузка списка игр
            });
        }
    }
}