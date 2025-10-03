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
        public bool? ShowDialog<TView>(TView view) where TView : class
        {
            // Create the dialog window
            Window window = new Window
            {
                Content = view, // Or use DataTemplate binding
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,                     
                Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive)
            };            
            return window.ShowDialog();
        }
    }


}
