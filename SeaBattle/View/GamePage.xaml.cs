using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SeaBattleWPF.API.Game;
using SeaBattleWPF.VM;

namespace SeaBattleWPF.View
{
    public partial class GamePage : Page
    {
        private GameVM _gameVM;

        public GamePage()
        {
            InitializeComponent();

            // Создаём GameVM и задаём DataContext
            _gameVM = new GameVM();
            this.DataContext = _gameVM;

            // Регистрируем dispatcher
            _gameVM.RegisterDispatcher(Dispatcher);

            // Регистрируем поля
            _gameVM.RegisterField(FieldUser1, true);   // Ваше поле
            _gameVM.RegisterField(FieldUser2, false);  // Поле противника
        }

        private void FieldUser2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _gameVM.ClickField((Canvas)sender, e);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            PageControl.GetInstance().CurrentPage = new PageListGames();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _gameVM?.Dispose();
        }
    }
}