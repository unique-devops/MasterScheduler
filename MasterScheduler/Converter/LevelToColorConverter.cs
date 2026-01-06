using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MasterScheduler.Converter
{
    public class LevelToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string level = value?.ToString() ?? "Information";
            return level switch
            {
                "Error" => Brushes.IndianRed,
                "Valid" => "#FFF36D",
                "Warning" => Brushes.Orange,
                _ => Brushes.CornflowerBlue
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
