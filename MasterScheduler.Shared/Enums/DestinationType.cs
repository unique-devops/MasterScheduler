using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Enums
{
    public enum DestinationType
    {
        LocalFolder,
        GoogleDrive,
        FTP,
        SFTP,
        OneDrive,
        AmazonS3,
        AzureBlob,
        NetworkShare
    }
}
