using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class GoogleDriveConfig : DestinationConfig
    {       
        public string FolderName { get; set; }      // Human-readable name (optional, for UI)
        public string UserEmail { get; set; }
        public string RefreshToken { get; set; }
        public string TargetFolderId { get; set; }
        public object FolderList { get; set; }

        public int RetentionDays { get; set; } = 0;
    }

}
