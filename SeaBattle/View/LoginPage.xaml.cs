using System.Windows.Controls;
using SeaBattle.VM;
namespace SeaBattle.View
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            // Передаем PasswordBox во ViewModel
            var vm = (LoginVM)DataContext;
            vm.SetPasswordBox(PasswordBox);
        }
    }
}