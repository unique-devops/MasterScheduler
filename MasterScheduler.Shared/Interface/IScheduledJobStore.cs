using MasterScheduler.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Interface
{
    public interface IScheduledJobStore
    {
        Task RunSqlBackupAsync(JobModel job, CancellationToken token);
    }
}
