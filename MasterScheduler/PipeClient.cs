using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasterScheduler
{
    public class PipeClient
    {
        public static async Task SendAsync(string obj)
        {
            using var client = new NamedPipeClientStream(".", "SchedulerPipe",
                PipeDirection.InOut, PipeOptions.Asynchronous);

            await client.ConnectAsync(5000);

            using var writer = new StreamWriter(client) { AutoFlush = true };           

            //string json = JsonSerializer.Serialize(obj);
            await writer.WriteLineAsync(obj);            
        }
    }
}
