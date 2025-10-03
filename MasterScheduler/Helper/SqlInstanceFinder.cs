using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Helper
{
    public static class SqlInstanceFinder
    {
        public static List<string> GetLocalSqlInstances()
        {
            var instances = new List<string>();

            // Default root
            string key = @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL";

            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(key))
            {
                if (rk != null)
                {
                    foreach (var name in rk.GetValueNames())
                    {
                        string instance = name == "MSSQLSERVER"
                            ? Environment.MachineName  // default instance
                            : $"{Environment.MachineName}\\{name}";
                        instances.Add(instance);
                    }
                }
            }

            return instances;
        }
    }
}
