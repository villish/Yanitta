using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Yanitta
{
    public class RelayCommand<T> : ICommand
    {
        Action<T> exec;
        Predicate<T> canexec;

        public RelayCommand(Action<T> exec, Predicate<T> canexec = null)
        {
            Debug.Assert(exec != null);
            this.exec = exec;
            this.canexec = canexec;
        }

        public event EventHandler CanExecuteChanged
        {
            add    { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter) => canexec?.Invoke((T)parameter) != false;

        public void Execute(object parameter) => exec((T)parameter);
    }
}
