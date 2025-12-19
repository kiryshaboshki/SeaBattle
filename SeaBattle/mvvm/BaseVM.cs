using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeaBattleWPF.mvvm
{
    public class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler OnPageClose;

        public void Signal([CallerMemberName] string prop = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        internal void OnClose()
        {
            OnPageClose?.Invoke(this, EventArgs.Empty);
        }
    }
}