using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Interface
{
    public interface IJobRepository
    {
        T? GetJobConfiguration<T>(int jobId);

        void UpdateJobConfiguration<T>(int jobId, T configuration);        
    }
}
