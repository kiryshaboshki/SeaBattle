using System.Windows.Controls;

namespace SeaBattle.View
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();

            // Передаем PasswordBox во ViewModel
            var vm = (VM.LoginVM)DataContext;
            vm.SetPasswordBox(PasswordBox);
        }
    }
}