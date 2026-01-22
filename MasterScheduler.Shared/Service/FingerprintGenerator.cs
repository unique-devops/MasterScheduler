using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    internal class FingerprintGenerator
    {
        public string GetId()
        {
            // Get CPU ID and Motherboard Serial
            string cpuId = GetWmiProperty("Win32_Processor", "ProcessorId");
            string motherBoard = GetWmiProperty("Win32_BaseBoard", "SerialNumber");

            // Combine them to form a unique string
            string rawId = $"PC:{cpuId}-{motherBoard}";

            // Hash the string so it looks like a professional ID (e.g., 8A3F-92B1-...)
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawId));
                string hex = BitConverter.ToString(bytes).Replace("-", "");
                return hex.Substring(0, 16); // Return first 16 chars
            }
        }
        private string GetWmiProperty(string table, string property)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT {property} FROM {table}"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj[property]?.ToString()?.Trim() ?? "Unknown";
                    }
                }
            }
            catch { return "NotFound"; }
            return "Unknown";
        }
    }
}
