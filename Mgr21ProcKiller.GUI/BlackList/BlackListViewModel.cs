using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreModel;
using CoreSecurityLib.Security;
using CoreSecurityLib.Service;
namespace Mgr21ProcKiller.GUI.BlackList
{
    public class BlackListViewModel:BindableBase
    {
        private ObservableCollection<ProcessModel> _blackList;
        private IOSecurity _ioSecurity = new IOSecurity();
        private LifetimeController _serviceLifetimeController = new LifetimeController();
        private bool _isError = false;

        public ObservableCollection<ProcessModel> BlackList
        {
            get
            {
                return _blackList;
            }
            set
            {
                OnPropertyChanged(ref _blackList, value, nameof(BlackList));
            }
        }
        public ProcessModel SelectedModel
        {
            get;set;
        }
        public RelayCommand ReloadCommand { get; private set; }
        public RelayCommand AddItemsCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand ClearCommand { get; private set; }
        public override event EventHandler<string> InfoRequested;
        public override event EventHandler<string> Error;
        public event EventHandler GoToAddItems;
        public BlackListViewModel()
        {
            ReloadCommand = new RelayCommand(OnReload, CanReload);
            AddItemsCommand = new RelayCommand(OnAddItems, CanAddItems);
            RemoveCommand = new RelayCommand(OnRemove, CanRemove);
            ClearCommand = new RelayCommand(OnClearList, CanClearList);
            BlackList = new ObservableCollection<ProcessModel>();
            CheckService();
        }

        private async void CheckService()
        {
            await Task.Run(() =>
            {
                _serviceLifetimeController.StartService();
            });
        }
        public async void Load()
        {
            OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code1"]);
            await Task.Run(() =>
            {

                DataResult<List<ProcessModel>> dataResult = _ioSecurity.RetrieveData();
                if (dataResult.MainResult.Success)
                {
                    BlackList = new ObservableCollection<ProcessModel>(dataResult.Data);
                    _isError = false;
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code2"]);
                }
                else
                {
                    BlackList = new ObservableCollection<ProcessModel>();
                    OnError(dataResult.MainResult.ErrorMessage);
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code3"]);
                }
            });
        }

        private void OnReload()
        {
            CheckService();
            Load();
        }

        private bool CanReload()
        {
            return true;
        }

        private async void OnClearList()
        {
            OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code4"]);
            await Task.Run(() =>
            {
                var result = _ioSecurity.StoreData(new List<ProcessModel>());
                if (result.Success)
                {
                    _isError = false;
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code5"]);
                    var refreshResult = _serviceLifetimeController.RefreshService();
                    if (refreshResult.Success)
                        OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code6"]);
                    else
                        OnError(refreshResult.ErrorMessage);
                }
                else
                {
                    OnError(result.ErrorMessage);
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code7"]);
                }
            });

            if(!_isError)
                BlackList.Clear();
        }

        private bool CanClearList()
        {
            return !_isError && BlackList.Count>0;
        }
        
        private async void OnRemove()
        {
            OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code8"]);
            ProcessModel processModel = SelectedModel;
            int index = BlackList.IndexOf(SelectedModel);
            BlackList.Remove(SelectedModel);
            await Task.Run(() =>
            {
                var result = _ioSecurity.StoreData(BlackList.ToList());
                if(result.Success)
                {
                    _isError = false;
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code9"]);
                    var refreshResult = _serviceLifetimeController.RefreshService();
                    if (refreshResult.Success)
                        OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code10"]);
                    else
                        OnError(refreshResult.ErrorMessage);
                }
                else
                {
                    OnError(result.ErrorMessage);
                    OnInfoRequested(LanguageChanger.Instance["BlackListVm_Code11"]);
                }

            });
            if(_isError)
            {
                BlackList.Insert(index, processModel);
            }
            
        }

        private bool CanRemove()
        {
            return !_isError && SelectedModel != null;
        }


        private void OnAddItems()
        {
            GoToAddItems?.Invoke(this, EventArgs.Empty);
        }

        private bool CanAddItems()
        {
            return !_isError;
        }
        private void OnInfoRequested(string info)
        {
            InfoRequested?.Invoke(this,info);
        }
        private void OnError(string error)
        {
            _isError = true;
            Error?.Invoke(this, error);
        }

    }
}
