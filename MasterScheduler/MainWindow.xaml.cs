using MasterScheduler.Service;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.ToastService.Register(GlobalToast);
            // Register show/hide actions
            LoaderService.Register(
                () => Dispatcher.Invoke(() => GlobalLoader.Visibility = Visibility.Visible),
                () => Dispatcher.Invoke(() => GlobalLoader.Visibility = Visibility.Collapsed)
            );
        }
    }
}