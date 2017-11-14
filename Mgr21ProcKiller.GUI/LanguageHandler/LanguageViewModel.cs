using CoreSecurityLib;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace Mgr21ProcKiller.GUI.LanguageHandler
{
    public class LanguageViewModel : INotifyPropertyChanged
    {
        private Language _selectedLanguage;
        private ObservableCollection<Language> _languages;
        public ObservableCollection<Language> Languages
        {
            get
            {
                return _languages;
            }
            set
            {
                _languages = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Languages)));
            }
        }
        public Language SelectedLanguage
        {
            get
            {
                return _selectedLanguage;
            }
            set
            {
                if (_selectedLanguage != value)
                {
                    _selectedLanguage = value;
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SelectedLanguage)));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public RelayCommand ChangeLanguageCommand { get; private set; }
        public LanguageViewModel()
        {
            Languages = new ObservableCollection<Language>
                {
                    new Language
                    {
                        Name = "English US",
                        ShortName = "en-US", //must be set to correct culture name
                        FlagPath = "/Contents/us-flag-100.jpg"
                    },
                    new Language
                    {
                        Name = "Indonesian",
                        ShortName = "id-ID", //must be set to correct culture name
                        FlagPath = "/Contents/id-flag-100.jpg"
                    }
                };
            ChangeLanguageCommand = new RelayCommand(OnLanguageChanged);
            HandleLoadLanguage();
        }

        private void HandleLoadLanguage()
        {
            if (LanguageChanger.Instance.CurrentCulture != null)
            {
                foreach (Language lang in Languages)
                {
                    if (lang.ShortName.Equals(LanguageChanger.Instance.CurrentCulture.Name,StringComparison.InvariantCultureIgnoreCase))
                    {
                        SelectedLanguage = lang;
                        break;
                    }
                }
            }
        }

        private void OnLanguageChanged()
        {
            if (SelectedLanguage != null)
            {
                LanguageChanger.Instance.CurrentCulture = new System.Globalization.CultureInfo(SelectedLanguage.ShortName);
                HandleSave();
            }
        }

        private async void HandleSave()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(Global.LanguageFileLocation))
                {
                    await writer.WriteAsync(SelectedLanguage.ShortName);
                }
            }
            catch { }
        }
    }
}
