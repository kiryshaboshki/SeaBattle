using SeaBattle.mvvm;
using System.Windows.Controls;
namespace SeaBattle.VM 
{
    public class PageControl : BaseVM
    {
        private static PageControl instance;

        private Page currentPage;
        public Page CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                Signal();
            }
        }

        private PageControl() { }

        public static PageControl GetInstance()
        {
            if (instance == null)
                instance = new PageControl();
            return instance;
        }
    }
}