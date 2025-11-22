using MasterScheduler.Shared.JobHelper;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker
{
    public class PipeServer
    {
        public async Task StartAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var server = new NamedPipeServerStream(
                    "SchedulerPipe",
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );

                await server.WaitForConnectionAsync(token);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var reader = new StreamReader(server);                       

                        string? line = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(line))
                        {
                            await FileCleanupHelper.RunFolderCleaner("D:\\EsUser\\Roshan\\Test");                            
                            await SendAsync(line);
                        }
                    }
                    catch (Exception ex)
                    {
                        // log if needed
                    }
                    finally
                    {
                        server.Dispose();   // IMPORTANT: do NOT use Disconnect() only
                    }

                }, CancellationToken.None);   // do NOT pass the main cancellation token
            }
        }


        public static async Task SendAsync(string id)
        {
            using var client = new NamedPipeClientStream(".", "SchedulerUI",
                PipeDirection.InOut, PipeOptions.Asynchronous);            

            try
            {
                // Try connect — if server not running, this throws TimeoutException
                await client.ConnectAsync(150);   // small timeout
            }
            catch
            {
                // Server not running → skip silently
                return;
            }

            try
            {
                using var writer = new StreamWriter(client) { AutoFlush = true };
                await writer.WriteLineAsync(id);
            }
            catch
            {
                // If server disconnected in middle → skip safely
            }
        }
    }
}
