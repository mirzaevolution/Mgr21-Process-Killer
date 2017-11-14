using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using CoreSecurityLib.Security;
using System.IO;
using CoreSecurityLib;

namespace Mgr21ProcKiller.GUI.Startup
{

    public partial class StartupView : MetroWindow
    {
        private IOSecurity _ioSecurity = new IOSecurity();
        public StartupView()
        {
            
            InitializeComponent();
            CheckStatus();
        }
        private void CheckStatus()
        {
            if(File.Exists(Global.LanguageFileLocation))
            {
                string lang = "";
                try
                {
                    lang = File.ReadAllText(Global.LanguageFileLocation);
                }
                catch { lang = "en-US"; }
                switch(lang.ToLower())
                {
                    case "en-us":
                        LanguageChanger.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        break;
                    case "id-id":
                        LanguageChanger.Instance.CurrentCulture = new System.Globalization.CultureInfo("id-ID");
                        break;
                    default:
                        LanguageChanger.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                        break;
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(Global.LanguageFileLocation, "en-US");
                }
                catch { }
                LanguageChanger.Instance.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            }

            if(File.Exists(Global.MasterKeyLocation))
            {
                passwordBoxLogin.Focus();
                gridLogin.Visibility = Visibility.Visible;
                gridSetupMasterKey.Visibility = Visibility.Collapsed;
            }
            else
            {
                passwordBoxNewPassword.Focus();
                gridSetupMasterKey.Visibility = Visibility.Visible;
                gridLogin.Visibility = Visibility.Collapsed;
            }
        }
        private async void OnSetUpMasterKey(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(passwordBoxNewPassword.Password) || String.IsNullOrEmpty(passwordBoxNewPasswordConfirm.Password))
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Error"], LanguageChanger.Instance["StartupView_CodeBehind_Code1"]);
            else if (!passwordBoxNewPassword.Password.Equals(passwordBoxNewPasswordConfirm.Password))
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Error"], LanguageChanger.Instance["StartupView_CodeBehind_Code2"]);
            else
                SetUpMasterKey();
        }

        private async void SetUpMasterKey()
        {
            string newPwd = passwordBoxNewPassword.Password;
            progressBarLoading.IsIndeterminate = true;
            bool success = true;
            string error = "";
            await Task.Run(() =>
            {
                var result = _ioSecurity.SetUpMasterKey(newPwd);
                if(!result.Success)
                {
                    success = false;
                    error = result.ErrorMessage;
                }
            });
            progressBarLoading.IsIndeterminate = false;
            if (success)
            {
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Success"], LanguageChanger.Instance["StartupView_CodeBehind_Code3"]);
                gridSetupMasterKey.Visibility = Visibility.Collapsed;
                gridLogin.Visibility = Visibility.Visible;
            }
            else
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Error"],error);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
        private int _loginCounter;
        private async void OnLogin(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(passwordBoxLogin.Password))
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Error"], LanguageChanger.Instance["StartupView_CodeBehind_Code4"]);
            else
            {

                if (_loginCounter < 5)
                    Login();
                else
                    App.Current.Shutdown();
            }
        }
        private async void Login()
        {
            _loginCounter++;
            bool success = false;
            string error = "";
            string pwd = passwordBoxLogin.Password;
            progressBarLoading.IsIndeterminate = true;
            await Task.Run(() =>
            {
                var login = _ioSecurity.Login(pwd);
                if (login.Success)
                    success = true;
                else
                    error = login.ErrorMessage;
            });
            progressBarLoading.IsIndeterminate = false;
            if (!success)
            {
                await this.ShowMessageAsync(LanguageChanger.Instance["StartupView_CodeBehind_Error"], error);
                loginStatus.Text = $"({_loginCounter}/5)";
            }
            else
            {
                MainWindow root = new MainWindow();
                root.Show();
                this.Close();
            }
        }
    }
}
