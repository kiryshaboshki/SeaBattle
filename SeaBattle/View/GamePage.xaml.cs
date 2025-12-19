using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

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

            // Инициализируем лог
            UpdateGameLog();
        }

        private void EnemyField_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var position = e.GetPosition(EnemyField);
            int x = (int)(position.X / 30);
            int y = (int)(position.Y / 30);

            if (x >= 0 && x < 10 && y >= 0 && y < 10)
            {
                var vm = (VM.GameVM)DataContext;
                vm.ProcessShot(x, y, true);
                UpdateGameLog();
            }
        }

        private void UpdateGameLog()
        {
            var vm = (VM.GameVM)DataContext;
            GameLog.Inlines.Clear();

            foreach (var logEntry in vm.GameLog)
            {
                var run = new Run(logEntry + "\n");

                // Цветовое кодирование сообщений
                if (logEntry.Contains("ПОБЕДА") || logEntry.Contains("ПОПАДАНИЕ") || logEntry.Contains("✓"))
                    run.Foreground = Brushes.Green;
                else if (logEntry.Contains("ПОРАЖЕНИЕ") || logEntry.Contains("☠"))
                    run.Foreground = Brushes.Red;
                else if (logEntry.Contains("Промах") || logEntry.Contains("◯"))
                    run.Foreground = Brushes.Blue;
                else
                    run.Foreground = Brushes.White;

                GameLog.Inlines.Add(run);
            }

            // Прокручиваем вниз
            GameLogScroll.ScrollToEnd();
        }

        private void SimulateEnemyTurn_Click(object sender, RoutedEventArgs e)
        {
            // Кнопка для демонстрации хода противника
            var vm = (VM.GameVM)DataContext;
            vm.SimulateEnemyTurn();
            UpdateGameLog();
        }
    }
}