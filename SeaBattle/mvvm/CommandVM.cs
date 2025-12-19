using System;
using System.Windows.Input;

namespace SeaBattleWPF.mvvm
{
    public class CommandVM : ICommand
    {
        Action action;

        public CommandVM(Action action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action();
        }
    }

    public class CommandVM<T> : ICommand where T : class
    {
        Action<T> action;

        public CommandVM(Action<T> action)
        {
            this.action = action;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action((T)parameter);
        }
    }
}