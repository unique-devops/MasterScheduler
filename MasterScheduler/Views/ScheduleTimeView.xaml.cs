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
     
        public ScheduleTimeModel ScheduleTime= new ScheduleTimeModel();

        ScheduleTimeViewModel vm = new ScheduleTimeViewModel();
        public ScheduleTimeView()
        {
            InitializeComponent();            
            this.DataContext = vm;            
        }
        

        private void HourTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var val = HourTextBox.Text;
            if (string.IsNullOrWhiteSpace(val))
            {
                HourTextBox.Text = DateTime.Now.Hour.ToString("D2");
            }
            else if (!int.TryParse(val, out int hour) || hour < 1 || hour > 24)
            {
                HourTextBox.Text = DateTime.Now.Hour.ToString("D2");
            }
        }

        private void MinuteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var val = MinuteTextBox.Text;
            if (string.IsNullOrWhiteSpace(MinuteTextBox.Text))
            {
                MinuteTextBox.Text = DateTime.Now.Minute.ToString("D2");
            }
            else if (!int.TryParse(val, out int min) || min < 1 || min > 60)
            {
                MinuteTextBox.Text = DateTime.Now.Minute.ToString("D2");
            }
        }

        private void MinuteTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(((TextBox)sender).Text + e.Text, out int value))
            {
                e.Handled = true;
                return;
            }
            
        }

        private void HourTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!int.TryParse(((TextBox)sender).Text + e.Text, out int value))
            {
                e.Handled = true;
                return;
            }
            
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ScheduleTime.Hour = vm.Hour;
            ScheduleTime.Minute = vm.Minute;
            ScheduleTime.EveryType = "minute";
            ScheduleTime.EveryTime = 1;
            this.DialogResult = true;
            Close();
        }
    }
}
