using System;
using System.Collections.Generic;
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
    /// Interaction logic for TimePickerControl.xaml
    /// </summary>
    public partial class TimePickerControl : UserControl
    {
        private enum TimePart { Hour, Minute }
        private TimePart selectedPart = TimePart.Hour;
        public TimePickerControl()
        {
            InitializeComponent();
            AmPmComboBox.SelectedIndex = 0; // Default to AM
            UpdateTextBoxesFromTime();
        }
        // DependencyProperty: Bindable Time
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register(nameof(Time), typeof(DateTime), typeof(TimePickerControl),
                new FrameworkPropertyMetadata(DateTime.Now.Date, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTimeChanged));

        public DateTime Time
        {
            get => (DateTime)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }
        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TimePickerControl control)
            {
                control.UpdateTextBoxesFromTime();
            }
        }

        private void UpdateTextBoxesFromTime()
        {
            int hour = Time.Hour;
            bool isPM = hour >= 12;

            if (hour == 0) hour = 12;
            else if (hour > 12) hour -= 12;

            HourTextBox.Text = hour.ToString("D2");
            MinuteTextBox.Text = Time.Minute.ToString("D2");
            AmPmComboBox.SelectedIndex = isPM ? 1 : 0;
        }

        private void UpdateTimeFromUI()
        {
            if (!int.TryParse(HourTextBox.Text, out int hour)) hour = 12;
            if (!int.TryParse(MinuteTextBox.Text, out int minute)) minute = 0;

            hour = Math.Clamp(hour, 1, 12);
            minute = Math.Clamp(minute, 0, 59);

            bool isPM = AmPmComboBox.SelectedIndex == 1;
            if (isPM && hour < 12) hour += 12;
            if (!isPM && hour == 12) hour = 0;

            Time = new DateTime(Time.Year, Time.Month, Time.Day, hour, minute, 0);
        }

        // Focus tracking
        private void HourTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedPart = TimePart.Hour;
        }

        private void MinuteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            selectedPart = TimePart.Minute;
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPart == TimePart.Hour)
            {
                int hour = int.Parse(HourTextBox.Text);
                hour = hour == 12 ? 1 : hour + 1;
                HourTextBox.Text = hour.ToString("D2");
            }
            else
            {
                int minute = int.Parse(MinuteTextBox.Text);
                minute = (minute + 1) % 60;
                MinuteTextBox.Text = minute.ToString("D2");
            }

            UpdateTimeFromUI();
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPart == TimePart.Hour)
            {
                int hour = int.Parse(HourTextBox.Text);
                hour = hour == 1 ? 12 : hour - 1;
                HourTextBox.Text = hour.ToString("D2");
            }
            else
            {
                int minute = int.Parse(MinuteTextBox.Text);
                minute = (minute - 1 + 60) % 60;
                MinuteTextBox.Text = minute.ToString("D2");
            }

            UpdateTimeFromUI();
        }

        private void AmPmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTimeFromUI();
        }
    }
}
