// MainVM.cs - должен быть в папке VM
using SeaBattleWPF.mvvm;
using SeaBattleWPF.View;
using System.Windows;

namespace SeaBattleWPF.VM
{
    public class MainVM : BaseVM
    {
        public PageControl PageControl { get; set; }

        public MainVM()
        {
            PageControl = PageControl.GetInstance();
            PageControl.CurrentPage = new LoginPage();
            Application.Current.Exit += PageControl.OnAppClose;
        }
    }
}