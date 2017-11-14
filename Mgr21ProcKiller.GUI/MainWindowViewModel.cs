using Mgr21ProcKiller.GUI.AddItems;
using Mgr21ProcKiller.GUI.BlackList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Mgr21ProcKiller.GUI
{
    public class MainWindowViewModel:BindableBase
    {
        private BlackListViewModel _blackListView = new BlackListViewModel();
        private AddItemsViewModel _addItemsView = new AddItemsViewModel();
        private BindableBase _currentView;

        public BindableBase CurrentView
        {
            get
            {
                return _currentView;
            }
            set
            {
                OnPropertyChanged(ref _currentView, value, nameof(CurrentView));
            }
        }
        
        public override event EventHandler<string> Error;
        public override event EventHandler<string> InfoRequested;
        public MainWindowViewModel()
        {
            _blackListView.Error += OnBlacListViewError;
            _blackListView.InfoRequested += OnBlackListViewInfoRequested;
            _blackListView.GoToAddItems += OnGoToAddItems;
            _addItemsView.Error += OnAddItemsViewError;
            _addItemsView.InfoRequested += OnAddItemsViewInfoRequested;
            _addItemsView.BackToMenu += OnBackToMenu;
            CurrentView = _blackListView;
        }

        private void OnBackToMenu(object sender, EventArgs e)
        {
            CurrentView = _blackListView;
        }

        private void OnGoToAddItems(object sender, EventArgs e)
        {
            CurrentView = _addItemsView;
        }

        private void OnAddItemsViewInfoRequested(object sender, string e)
        {
            OnInfoRequested(e);
        }

        private void OnAddItemsViewError(object sender, string e)
        {
            OnError(e);
        }

        private void OnBlackListViewInfoRequested(object sender, string e)
        {
            OnInfoRequested(e);
        }

        private void OnBlacListViewError(object sender, string e)
        {
            OnError(e);
        }

        private void OnInfoRequested(string info)
        {
            InfoRequested?.Invoke(this, info);
        }
        private void OnError(string error)
        {
            Error?.Invoke(this, error);
        }
    }
}
