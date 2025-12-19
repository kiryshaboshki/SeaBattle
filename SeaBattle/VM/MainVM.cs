using SeaBattle.mvvm;
using SeaBattle.View;

namespace SeaBattle.VM
{
    public class MainVM : BaseVM
    {
        public PageControl PageControl { get; set; }

        public MainVM()
        {
            PageControl = PageControl.GetInstance();
            PageControl.CurrentPage = new LoginPage();
        }
    }
}