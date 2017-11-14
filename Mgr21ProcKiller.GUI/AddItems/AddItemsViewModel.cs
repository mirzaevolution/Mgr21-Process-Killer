using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreModel;
using CoreSecurityLib.Security;
using CoreSecurityLib.Service;
using Microsoft.Win32;
namespace Mgr21ProcKiller.GUI.AddItems
{
    public class AddItemsViewModel:BindableBase
    {
        private IOSecurity _ioSecurity;
        private LifetimeController _serviceLifetimeController;
        private ObservableCollection<string> _itemsToAdd;
        
        public override event EventHandler<string> InfoRequested;
        public override event EventHandler<string> Error;
        public event EventHandler BackToMenu;
        public string SelectedItem
        {
            get;set;
        }
        public ObservableCollection<string> ItemsToAdd
        {
            get
            {
                return _itemsToAdd;
            }
            set
            {
                OnPropertyChanged(ref _itemsToAdd, value, nameof(ItemsToAdd));
            }
        }
        public RelayCommand BrowseCommand { get; private set; }
        public RelayCommand AddCommand { get; private set; }
        public RelayCommand BackToMenuCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }

        public AddItemsViewModel()
        {
            _ioSecurity = new IOSecurity();
            _serviceLifetimeController = new LifetimeController();
            BrowseCommand = new RelayCommand(OnBrowse);
            AddCommand = new RelayCommand(OnAdd, CanAdd);
            BackToMenuCommand = new RelayCommand(OnBackToMenu);
            RemoveCommand = new RelayCommand(OnRemove, CanRemove);
            ItemsToAdd = new ObservableCollection<string>();

        }
        
        private void OnBrowse()
        {
           OpenFileDialog openFile = new OpenFileDialog()
           {
               FileName = "",
               Filter = "Executable File (*.exe)|*.exe",
               CheckFileExists = true,
               CheckPathExists = true,
               Multiselect = true
           };
            if(openFile.ShowDialog().Value)
            {
                foreach (var name in openFile.FileNames)
                    ItemsToAdd.Add(name);
            }
        }

        private async void OnAdd()
        {
            
            OnInfoRequested(LanguageChanger.Instance["AddItemsVm_Code1"]);
     

            await Task.Run(() =>
            {
                var result = _ioSecurity.RetrieveData();
                if (result.MainResult.Success)
                {
                    List<ProcessModel> list = (from item in ItemsToAdd
                                               select _ioSecurity.CreateProcessModel(item).Data).ToList();
                    int itemsAdded = 0;
                    foreach(ProcessModel model in list)
                    {
                        if (!result.Data.Any(x => x.ProcessName.Equals(model.ProcessName, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            result.Data.Add(model);
                            itemsAdded++;
                        }
                    }
                    if (itemsAdded > 0)
                    {
                        var addResult = _ioSecurity.StoreData(result.Data);
                        if (addResult.Success)
                        {
             
                            OnInfoRequested(LanguageChanger.Instance["AddItemsVm_Code2"]);
                            var refreshResult = _serviceLifetimeController.RefreshService();
                            if (refreshResult.Success)
                                OnInfoRequested(LanguageChanger.Instance["AddItemsVm_Code3"]);
                            else
                                OnError(refreshResult.ErrorMessage);
                        }
                        else
                            OnError(addResult.ErrorMessage);
                    }
                    else
                    {
                    
                        OnInfoRequested(LanguageChanger.Instance["AddItemsVm_Code4"]);
                    }
                }
                else
                    OnError(LanguageChanger.Instance["AddItemsVm_Code5"]);
               
            });

            ItemsToAdd.Clear();
        }
        private bool CanRemove()
        {
            return SelectedItem != null;
        }
        private void OnRemove()
        {
            ItemsToAdd.Remove(SelectedItem);
        }

        private bool CanAdd()
        {
            return ItemsToAdd.Count > 0;
        }

        private void OnBackToMenu()
        {
            BackToMenu?.Invoke(this, EventArgs.Empty);    
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
