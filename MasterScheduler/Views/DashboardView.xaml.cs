using MasterScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for DashboardView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();            
        }
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            var media = sender as MediaElement;
            media.Position = TimeSpan.Zero;
            media.Play();
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Delete == e.Key)
            {                
                var vm = (DashboardViewModel)this.DataContext;                
                if (vm.SelectedJob?.Status != "Running")
                {                   
                    vm.DeleteJob();                    
                    dataGrid.Focus();                    
                }               
            }            
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
