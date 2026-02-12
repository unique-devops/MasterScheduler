using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MasterScheduler.Converter
{
    public class JobTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value?.ToString() ?? "Default";

            // Define your paths here
            string iconPath = type switch
            {
                "SQLBACKUP" => "/Assets/sql-server.png",
                "CLEANUP" => "/Assets/delete.png",
                "FIELSYNC" => "/Assets/sync.png",
                _ => "/Assets/task.png"
            };

            try
            {
                return iconPath;
            }
            catch
            {
                iconPath = "";
                return iconPath;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
