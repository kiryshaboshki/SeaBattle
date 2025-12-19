using System;
using System.Windows.Input;

namespace SeaBattle.mvvm
{
    public class CommandVM : ICommand
    {
        Action action;

        public CommandVM(Action action)
        {
            this.action = action;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => action();
    }
}