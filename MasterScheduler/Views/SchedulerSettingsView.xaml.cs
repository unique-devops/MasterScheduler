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
        private bool _isClosing =false;
        public SchedulerSettingsView()
        {
            InitializeComponent();
            var vm  = new SchedulerSettingsViewModel();
            DataContext = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SchedulerSettingsViewModel.ShouldClose))
            {
                var vm = (SchedulerSettingsViewModel)sender;
                if (vm.ShouldClose && !_isClosing)
                {
                    this.DialogResult =true;
                }
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosing = true;
            base.OnClosing(e);
        }
    }
}
