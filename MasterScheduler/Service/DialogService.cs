using CommunityToolkit.Mvvm.ComponentModel;
using MasterScheduler.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MasterScheduler.Service
{
    public class DialogService : IDialogService
    {
        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : ObservableObject
        {
            // 1. Create the 'Shell' Window
            var shell = new Window
            {
                Title = "Configuration", // You can bind this to VM.Title if needed
                Content = viewModel,      // The DataTemplate handles the UI look
                SizeToContent = SizeToContent.WidthAndHeight,                
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = Application.Current.MainWindow,
                ResizeMode = ResizeMode.NoResize
            };

            // 2. Listen for Close request from ViewModel
            if (viewModel is IClosableDialog closable)
            {
                closable.RequestClose += (result) =>
                {
                    shell.DialogResult = result;
                    shell.Close();
                };
            }

            return shell.ShowDialog();
        }
    }


}
