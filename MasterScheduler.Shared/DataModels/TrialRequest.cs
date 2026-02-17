using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class TrialRequest
    {       
        public string AppName { get; set; }     
        public string Email { get; set; }        
        public string DeviceId { get; set; }
    }
}
