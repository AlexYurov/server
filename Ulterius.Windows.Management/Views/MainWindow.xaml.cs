using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Ulterius.Windows.Management.ViewModels;

namespace Ulterius.Windows.Management
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LogTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Dispatcher.Invoke(() => LogTextBox.ScrollToEnd(), DispatcherPriority.ApplicationIdle);
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var vm = (MainViewModel) DataContext;
            vm.RefreshCommand.Execute(null);
        }
    }
}
