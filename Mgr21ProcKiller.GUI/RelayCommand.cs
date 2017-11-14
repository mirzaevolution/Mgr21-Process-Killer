using System;
using System.Windows.Input;
namespace Mgr21ProcKiller.GUI
{
    public class RelayCommand : ICommand
    {
        private Action _action;
        private Func<bool> _func;

        public RelayCommand(Action action)
        {
            _action = action;
        }
        public RelayCommand(Action action, Func<bool> func)
        {
            _action = action;
            _func = func;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            if (_func != null)
                return _func();
            if (_action != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private Action<T> _action;
        private Func<T,bool> _func;
        public RelayCommand(Action<T> action)
        {
            _action = action;
        }
        public RelayCommand(Action<T> action, Func<T,bool> func)
        {
            _action = action;
            _func = func;
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }


        public bool CanExecute(object parameter)
        {
            if (_func != null)
                return _func((T)parameter);
            if (_action != null)
                return true;
            return false;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }
    }
}
