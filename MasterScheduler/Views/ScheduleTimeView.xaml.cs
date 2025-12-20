using MasterScheduler.Models;
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
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for ScheduleTimeView.xaml
    /// </summary>
    public partial class ScheduleTimeView : Window
    {
        private bool _isClosing = false;
        public ScheduleTimeModel ScheduleTime= new ScheduleTimeModel();
        public ScheduleTimeView()
        {
            InitializeComponent();
            var vm = new ScheduleTimeViewModel();
            this.DataContext = vm;
            vm.PropertyChanged += Vm_PropertyChanged;
        }
        private void Vm_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ScheduleTimeViewModel.ShouldClose))
            {
                var vm = (ScheduleTimeViewModel)sender;
                if (vm.ShouldClose && !_isClosing)
                {
                    ScheduleTime.Hour = vm.Hour;
                    ScheduleTime.Minute = vm.Minute;
                    ScheduleTime.EveryType = "minute";
                    ScheduleTime.EveryTime = 1;                    
                    this.DialogResult = true;
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
