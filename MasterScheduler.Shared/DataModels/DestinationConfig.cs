using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    [JsonDerivedType(typeof(LocalFolderConfig), typeDiscriminator: "local")]
    [JsonDerivedType(typeof(GoogleDriveConfig), typeDiscriminator: "gdrive")]
    public abstract class DestinationConfig
    {
        public string DisplayName { get; set; }
        public bool UseProxy { get; set; }
        public int TimeoutSeconds { get; set; } = 60;
    }

}
