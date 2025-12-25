using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public static class Cipher
    {
        // High-level encryption for local machine/user
        public static string Protect(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;
            byte[] data = Encoding.UTF8.GetBytes(text);
            byte[] encrypted = ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encrypted);
        }

        public static string Unprotect(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText)) return null;
            byte[] data = Convert.FromBase64String(encryptedText);
            byte[] decrypted = ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decrypted);
        }
    }
}
