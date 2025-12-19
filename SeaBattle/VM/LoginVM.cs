using SeaBattle.API;
using SeaBattle.mvvm;
using System.Threading.Tasks;
using System.Windows;

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
            LoginCommand = new CommandVM(async () =>
            {
                if (string.IsNullOrEmpty(LoginText) || passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
                {
                    MessageBox.Show("Введите логин и пароль");
                    return;
                }

                var result = await Client.Instance.PostAsync(
                    $"Auth/GetToken?login={LoginText}&password={passwordBox.Password}");

                if (result.Success)
                {
                    Client.Instance.SetToken(result.Response);

                    // Переход на страницу списка игр
                    var pageControl = PageControl.GetInstance();
                    pageControl.CurrentPage = new View.PageListGames();
                }
                else
                {
                    MessageBox.Show($"Ошибка входа: {result.Response}");
                }
            });

            RegistrationCommand = new CommandVM(async () =>
            {
                if (string.IsNullOrEmpty(LoginText) || passwordBox == null || string.IsNullOrEmpty(passwordBox.Password))
                {
                    MessageBox.Show("Введите логин и пароль для регистрации");
                    return;
                }

                var result = await Client.Instance.PostAsync("Auth/Registration",
                    new { Login = LoginText, Password = passwordBox.Password });

                if (result.Success)
                {
                    MessageBox.Show("Регистрация успешна! Выполняется вход...");
                    // Автоматически выполняем вход после регистрации
                    LoginCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show($"Ошибка регистрации: {result.Response}");
                }
            });
        }

        private System.Windows.Controls.PasswordBox passwordBox;
        public void SetPasswordBox(System.Windows.Controls.PasswordBox box)
        {
            passwordBox = box;
        }
    }
}