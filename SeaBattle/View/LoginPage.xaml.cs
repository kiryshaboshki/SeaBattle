using SeaBattle.API;
using SeaBattle.VM;
using System.Windows;
using System.Windows.Controls;
using SeaBattle; // ДОБАВИТЬ ЭТОТ USING

namespace SeaBattle.View
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            var vm = (LoginVM)DataContext;
            vm.SetPasswordBox(PasswordBox);
        }

        private void OfflineMode_Click(object sender, RoutedEventArgs e)
        {
            CurrentGame.IsOnline = false;
            CurrentGame.OpponentName = "Компьютер";
            CurrentGame.Id = 999;

            var pageControl = PageControl.GetInstance();
            pageControl.CurrentPage = new ShipPlacementPage();

            MessageBox.Show("Оффлайн режим активирован!\nВы играете против компьютера.",
                "Быстрый старт", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}