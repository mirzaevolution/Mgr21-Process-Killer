using System;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Mgr21ProcKiller.GUI.Settings;
using Mgr21ProcKiller.GUI.About;
namespace Mgr21ProcKiller.GUI
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        

        private void OnInfoRequested(object sender, string e)
        {
            Dispatcher.Invoke(() =>
            {
                statusbarInfo.Content = e;
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void OnError(object sender, string e)
        {
            Dispatcher.Invoke(async() =>
            {
                await this.ShowMessageAsync("Error", e);
            }, System.Windows.Threading.DispatcherPriority.Background);
        }

        private void ButtonSettingsShow(object sender, RoutedEventArgs e)
        {
            new SettingsView().ShowDialog();

        }

        private void ButtonAboutShow(object sender, RoutedEventArgs e)
        {
            new AboutPage().ShowDialog();
        }
    }
}
