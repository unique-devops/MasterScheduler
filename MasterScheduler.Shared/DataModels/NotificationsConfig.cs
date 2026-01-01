using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class NotificationsConfig
    {
        public string EmailOnSuccess { get; set; }
        public string EmailOnFailure { get; set; }
        public string WebhookUrl { get; set; }
    }
}
