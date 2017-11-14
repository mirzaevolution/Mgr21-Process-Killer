using System.ComponentModel;
using System.Globalization;
using System.Resources;
namespace Mgr21ProcKiller.GUI
{
    public class LanguageChanger : INotifyPropertyChanged
    {
        private static LanguageChanger _instance = new LanguageChanger();
        private readonly ResourceManager _manager = Properties.Resources.ResourceManager;
        public static LanguageChanger Instance
        {
            get
            {
                return _instance;
            }
        }
        private CultureInfo _cultureInfo;
        public string this[string key]
        {
            get
            {
                return _manager.GetString(key, _cultureInfo);
            }
        }
        public CultureInfo CurrentCulture
        {
            get
            {
                return _cultureInfo;
            }
            set
            {
                if(_cultureInfo!=value)
                {
                    _cultureInfo = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(""));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged=delegate { };
    }
}
