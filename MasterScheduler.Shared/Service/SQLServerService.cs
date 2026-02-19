using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    public class SQLServerService
    {
        public async Task<bool> IsSupportNativeCompressionAsync(string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync();
           
            var checkSql = "SELECT 1 FROM sys.configurations WHERE name = 'backup compression default'";
            using var checkCmd = new SqlCommand(checkSql, conn);
            bool supportsNativeCompression = checkCmd.ExecuteScalar() != null;

            return supportsNativeCompression;
        }

        public bool IsSupportNativeCompression(string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            var checkSql = "SELECT 1 FROM sys.configurations WHERE name = 'backup compression default'";
            using var checkCmd = new SqlCommand(checkSql, conn);
            bool supportsNativeCompression = checkCmd.ExecuteScalar() != null;

            return supportsNativeCompression;
        }
    }
}
