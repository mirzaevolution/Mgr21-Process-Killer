using System;
using System.ComponentModel;

namespace Mgr21ProcKiller.GUI
{
    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual event EventHandler<string> InfoRequested = delegate { };
        public virtual event EventHandler<string> Error = delegate { };
        protected virtual void OnPropertyChanged<T>(ref T member, T value, string propertyName)
        {
            if (Equals(member, value))
                return;
            member = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
