using SeaBattleWPF.mvvm;
using System;
using System.Windows;
using System.Windows.Controls;

namespace SeaBattleWPF.VM
{
    public class PageControl : BaseVM
    {
        static PageControl instance;

        private Page currentPage;

        public Page CurrentPage
        {
            get => currentPage;
            set
            {
                if (currentPage != null)
                {
                    try
                    {
                        OnAppClose(this, null);
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку, но не падаем
                        System.Diagnostics.Debug.WriteLine($"Error in OnAppClose: {ex.Message}");
                    }
                }

                currentPage = value;
                Signal();
            }
        }

        internal static PageControl GetInstance()
        {
            if (instance == null)
                instance = new PageControl();
            return instance;
        }

        internal void OnAppClose(object sender, ExitEventArgs e)
        {
            try
            {
                if (CurrentPage?.DataContext is BaseVM vm)
                {
                    vm.OnClose();
                }
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение
                System.Diagnostics.Debug.WriteLine($"Error closing page: {ex.Message}");
            }
        }

        // Альтернативный метод для явного закрытия страницы
        internal void CloseCurrentPage()
        {
            if (CurrentPage?.DataContext is BaseVM vm)
            {
                vm.OnClose();
            }
        }
    }
}