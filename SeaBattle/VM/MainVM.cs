using SeaBattle.mvvm;

namespace SeaBattle.VM
{
    public class MainVM : BaseVM
    {
        private string title = "Морской бой";
        public string Title
        {
            get => title;
            set { title = value; Signal(); }
        }

        public MainVM()
        {
        }
    }
}