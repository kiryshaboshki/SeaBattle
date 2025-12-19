using SeaBattleWPF.VM;
using System.Windows;
using System.Windows.Controls;

namespace SeaBattleWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Создаём MainVM и задаём DataContext
            var mainVM = new MainVM();
            this.DataContext = mainVM;

            // Привязываем Frame к PageControl
            MainFrame.SetBinding(Frame.ContentProperty,
                new System.Windows.Data.Binding("PageControl.CurrentPage")
                {
                    Source = mainVM
                });
        }
    }
}