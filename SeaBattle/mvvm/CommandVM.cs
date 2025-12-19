using System;
using System.Windows.Input;

namespace SeaBattle.mvvm
{
    public class CommandVM : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        public CommandVM(Action execute) : this(execute, null) { }

        public CommandVM(Action execute, Func<bool> canExecute)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            execute();
        }
    }
}