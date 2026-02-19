using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MasterScheduler.Shared.Service
{
    public class FileCompressionHelper
    {
        public static async Task ZipCompressAsync(string zipPath, string bakFilePath, CancellationToken token)
        {
            await Task.Run(() =>
            {
                using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {                    
                    archive.CreateEntryFromFile(bakFilePath, Path.GetFileName(bakFilePath), CompressionLevel.Optimal);
                }
            }, token);

            if (File.Exists(bakFilePath)) File.Delete(bakFilePath);
        }
    }
}
