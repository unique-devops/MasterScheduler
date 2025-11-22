using MasterScheduler.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public static class FileCleanupHelper
    {
        public static void RunFileCleanup(JobModel job)
        {
            // placeholder example for next job type
        }

        public static async Task RunFolderCleaner(string path)
        {
            if (!Directory.Exists(path))
                return;

            await Task.Run(() =>
            {
                string[] files = Directory.GetFiles(path, "*.txt");

                foreach (var file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        // optional: log error
                        Console.WriteLine($"Failed to delete {file}: {ex.Message}");
                    }
                }
            });
            await Task.Delay(5000);
        }
    }
}
