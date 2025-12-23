using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using static MasterScheduler.Models.Enums;

namespace MasterScheduler.Converter
{
    public class DestinationIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DestinationType type)
            {
                return type switch
                {
                    DestinationType.LocalFolder => "/Assets/folder.png",
                    DestinationType.GoogleDrive => "/Assets/google-drive.png",
                    DestinationType.FTP => "/Assets/ftp.png",
                    DestinationType.SFTP => "/Assets/sftp.png",
                    DestinationType.OneDrive => "/Assets/onedrive.png",
                    DestinationType.AmazonS3 => "/Assets/s3.png",
                    DestinationType.AzureBlob => "/Assets/azure.png",
                    DestinationType.NetworkShare => "/Assets/network.png",
                    _ => "/Assets/default.png"
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
