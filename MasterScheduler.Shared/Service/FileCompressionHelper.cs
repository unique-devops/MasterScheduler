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
                if (File.Exists(zipPath))
                {
                    using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
                    {
                        string entryName = Path.GetFileName(bakFilePath);

                        // Remove existing entry if exists
                        var existingEntry = archive.GetEntry(entryName);
                        existingEntry?.Delete();

                        // Add new file
                        archive.CreateEntryFromFile(
                            bakFilePath,
                            entryName,
                            CompressionLevel.Optimal
                        );
                    }
                }
                else
                {
                    using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                    {
                        archive.CreateEntryFromFile(bakFilePath, Path.GetFileName(bakFilePath), CompressionLevel.Optimal);
                    }
                }                    
            }, token);

            if (File.Exists(bakFilePath)) File.Delete(bakFilePath);
        }
    }
}
