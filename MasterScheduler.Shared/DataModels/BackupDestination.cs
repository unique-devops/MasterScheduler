using MasterScheduler.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    //public class BackupDestination<T> where T : class
    //{
    //    public Guid Id { get; set; }
    //    public DestinationType Type { get; set; }
    //    public string DisplayText { get; set; }
    //    public T? Config { get; set; }
    //}
    // The non-generic base class
    
    public class BackupDestination
    {
        public Guid Id { get; set; }
        public DestinationType Type { get; set; }
        public string DisplayText { get; set; }
        public string Status { get; set; }
        public string ResumeUri { get; set; }
        public DestinationConfig Config { get; set; }
    }

}
