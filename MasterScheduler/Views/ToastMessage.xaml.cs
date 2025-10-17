using MasterScheduler.Shared.Enums;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for ToastMessage.xaml
    /// </summary>
    public partial class ToastMessage : UserControl
    {
        public ToastMessage()
        {
            InitializeComponent();
        }
        public async Task ShowAsync(string message, ToastType type = ToastType.Info, int durationMs = 2500)
        {
            MessageText.Text = message;
            SetToastStyle(type);

            ToastRoot.Visibility = Visibility.Visible;

            // Fade-in animation
            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            ToastRoot.BeginAnimation(OpacityProperty, fadeIn);

            await Task.Delay(durationMs);

            // Fade-out animation
            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            ToastRoot.BeginAnimation(OpacityProperty, fadeOut);

            await Task.Delay(300);
            ToastRoot.Visibility = Visibility.Collapsed;
        }

        private void SetToastStyle(ToastType type)
        {
            switch (type)
            {
                case ToastType.Success:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(56, 142, 60));  // Green
                    IconText.Text = "✔";
                    IconText.Foreground = Brushes.White;
                    break;
                case ToastType.Error:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(211, 47, 47));  // Red
                    IconText.Text = "❌";
                    IconText.Foreground = Brushes.White;
                    break;
                case ToastType.Warning:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(255, 143, 0));  // Orange
                    IconText.Text = "⚠";
                    IconText.Foreground = Brushes.White;
                    break;
                default:
                    ToastBorder.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue
                    IconText.Text = "ℹ";
                    IconText.Foreground = Brushes.White;
                    break;
            }
        }
    }
}
