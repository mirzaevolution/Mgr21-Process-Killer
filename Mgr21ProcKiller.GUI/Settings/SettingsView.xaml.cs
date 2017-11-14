using System;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using CoreSecurityLib.Common;
using CoreSecurityLib.Service;
using System.Windows.Threading;
using CoreSecurityLib.Security;
using CoreModel;
using CoreSecurityLib;
using System.IO;

namespace Mgr21ProcKiller.GUI.Settings
{
    public partial class SettingsView : MetroWindow
    {
        LifetimeController _serviceLifetimeController;
        CoreModel.Settings _settings;
        public SettingsView()
        {
            
            InitializeComponent();
            _serviceLifetimeController = new LifetimeController();
            LoadDefaultSettings();
        }

        private void LoadDefaultSettings()
        {
            Dispatcher.Invoke(async() =>
            {
                var loadResult = Config.RetrieveSettings();
                if(loadResult.MainResult.Success)
                {
                    _settings = loadResult.Data;
                    switch(_settings.ComparisonType)
                    {
                        case CoreModel.ComparisonType.By2KBInitialBytes:
                            radioButtonBy2KbInitialBytes.IsChecked = true;
                            break;
                        case CoreModel.ComparisonType.ByName:
                            radioButtonByName.IsChecked = true;
                            break;
                    }
                    numericUpDownInterval.Value = _settings.Interval >= 500 ? _settings.Interval : 1000;
                }
                else
                {
                    
                    buttonUpdateSettings.IsEnabled = buttonChangePassword.IsEnabled = false;
                    await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], loadResult.MainResult.ErrorMessage);
                }
            },DispatcherPriority.Background);
        }

        private void ExitWindow(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void UpdateBehaviorSettings(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioButtonBy2KbInitialBytes.IsChecked.Value)
                    _settings.ComparisonType = CoreModel.ComparisonType.By2KBInitialBytes;
                else if (radioButtonByName.IsChecked.Value)
                    _settings.ComparisonType = CoreModel.ComparisonType.ByName;
                _settings.Interval = (int)numericUpDownInterval.Value;
                await Dispatcher.Invoke(async () =>
                 {
                     var updateResult = Config.StoreSettings(_settings);
                     if (updateResult.Success)
                     {
                         var refreshResult = _serviceLifetimeController.RefreshService();
                         if (refreshResult.Success)
                             await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Success"], LanguageChanger.Instance["SettingsView_CodeBehind_Code1"]);
                         else
                             await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"],
                                     LanguageChanger.Instance["SettingsView_CodeBehind_Code2"]);
                     }
                     else
                         await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], updateResult.ErrorMessage);

                 }, DispatcherPriority.Background);
            }
            catch(Exception ex)
            {
                await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], ex.ToString());
            }
        }

        private async void UpdateMasterPassword(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(passwordBoxCurrentPassword.Password) || String.IsNullOrEmpty(passwordBoxNewPassword.Password))
                await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], LanguageChanger.Instance["SettingsView_CodeBehind_Code3"]);
            else if (passwordBoxCurrentPassword.Password.Length <= 5 || passwordBoxNewPassword.Password.Length <= 5)
                await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], LanguageChanger.Instance["SettingsView_CodeBehind_Code4"]);
            else if (passwordBoxCurrentPassword.Password.Equals(passwordBoxNewPassword.Password))
                await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], LanguageChanger.Instance["SettingsView_CodeBehind_Code5"]);
            else if (!File.Exists(Global.MasterKeyLocation))
                await this.ShowMessageAsync(LanguageChanger.Instance["SettingsView_CodeBehind_Error"], LanguageChanger.Instance["SettingsView_CodeBehind_Code6"]);
            else
            {
                UpdatePassword();
            }
        }
        private async void UpdatePassword()
        {
            IOSecurity iOSecurity = new IOSecurity();
            //because we use non-UI thread to update password,
            //we must copy both passwords in variables.
            string oldPwd = passwordBoxCurrentPassword.Password;
            string newPwd = passwordBoxNewPassword.Password;
            string message = "";
            string title = "";
            progressBarLoading.IsIndeterminate = true;
            await Task.Run(() =>
            {
                MainResult loginResult = iOSecurity.Login(oldPwd);
                if (loginResult.Success)
                {
                    MainResult changePasswordResult = iOSecurity.ChangeMasterKey(newPwd);
                    if (changePasswordResult.Success)
                    {
                        title = LanguageChanger.Instance["SettingsView_CodeBehind_Success"];
                        message = LanguageChanger.Instance["SettingsView_CodeBehind_Code7"];
                    }
                    else
                    {
                        title = LanguageChanger.Instance["SettingsView_CodeBehind_Error"];
                        message = changePasswordResult.ErrorMessage;
                    }
                }
                else
                {
                    title = LanguageChanger.Instance["SettingsView_CodeBehind_Error"];
                    message = LanguageChanger.Instance["SettingsView_CodeBehind_Code8"];
                }
            });

            progressBarLoading.IsIndeterminate = false;
            await this.ShowMessageAsync(title, message);
            passwordBoxCurrentPassword.Password = passwordBoxNewPassword.Password = string.Empty;
        }
    }
}
