using MahApps.Metro.Controls;
using System.Windows;

namespace Mgr21ProcKiller.GUI.About
{
    public partial class AboutPage : MetroWindow
    {
        public AboutPage()
        {
            InitializeComponent();
        }
        private void OnDrag(object sender, DragEventArgs e)
        {
            this.DragMove();
        }

        private void OnNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
        }

        private void OnExit(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
