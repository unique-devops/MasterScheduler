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

        public static List<string> GetAllLocalSqlInstances()
        {
            HashSet<string> instances = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string machineName = Environment.MachineName;

            // List of registry paths where SQL Server stores instance names
            string[] registryPaths = {
                        @"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL",
                        @"SOFTWARE\Wow6432Node\Microsoft\Microsoft SQL Server\Instance Names\SQL"
                    };

            foreach (var path in registryPaths)
            {
                using (RegistryKey localMachine = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                       Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32))
                {
                    using (RegistryKey key = localMachine.OpenSubKey(path))
                    {
                        if (key != null)
                        {
                            foreach (string instanceName in key.GetValueNames())
                            {
                                if (instanceName.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                                    instances.Add(machineName); // Default Instance
                                else
                                    instances.Add($@"{machineName}\{instanceName}"); // Named Instance
                            }
                        }
                    }
                }
            }
            return instances.ToList();
        }
    }
}
