using SeaBattle.mvvm;
using SeaBattle.View;
using System.Windows;
using System.Windows.Controls;

namespace SeaBattle.VM
{
    public class LoginVM : BaseVM
    {
        private string loginText;
        public string LoginText
        {
            get => loginText;
            set { loginText = value; Signal(); }
        }

        public CommandVM LoginCommand { get; set; }
        public CommandVM RegistrationCommand { get; set; }

        public LoginVM()
        {
            LoginCommand = new CommandVM(() =>
            {
                MessageBox.Show($"Вход для пользователя: {LoginText}");
                // Здесь позже будет переход на другую страницу
            });

            RegistrationCommand = new CommandVM(() =>
            {
                MessageBox.Show($"Регистрация для пользователя: {LoginText}");
            });
        }

        private PasswordBox passwordBox;
        public void SetPasswordBox(PasswordBox box)
        {
            passwordBox = box;
        }
    }
}