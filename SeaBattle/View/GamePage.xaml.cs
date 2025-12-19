using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeaBattle.View
{
    public partial class GamePage : Page
    {
        public GamePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Инициализируем поля при загрузке страницы
            var vm = (VM.GameVM)DataContext;
            vm.InitializeFields(MyField, EnemyField);
        }

        private void EnemyField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(EnemyField);
            int x = (int)(position.X / 30);
            int y = (int)(position.Y / 30);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
            {
                MessageBox.Show($"Выстрел в клетку: {(char)('A' + x)}{y + 1}");
                // Здесь будет логика выстрела
            }
        }
    }
}