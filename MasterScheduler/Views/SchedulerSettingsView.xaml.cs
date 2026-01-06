using MasterScheduler.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for SchedulerSettingsView.xaml
    /// </summary>
    public partial class SchedulerSettingsView : Window
    {
        SchedulerSettingsViewModel vm = new SchedulerSettingsViewModel();
        public SchedulerSettingsView()
        {
            InitializeComponent();            
            DataContext = vm;
            if (DataContext is SchedulerSettingsViewModel vm1)
            {
                // Define what happens when the ViewModel calls CloseAction
                vm1.CloseAction = (result) =>
                {
                    this.DialogResult = result;
                    this.Close();
                };
            }
        }
    }
}
