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
        private readonly Action<int> _onCancelRequested;
        private const string PipeName = "JobControlPipe";

        public PipeServer(Action<int> onCancelRequested)
        {
            _onCancelRequested = onCancelRequested;
        }
        public async Task StartAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Create a new pipe instance for each connection
                    using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                    // Wait for the UI/Pipeline to connect
                    await server.WaitForConnectionAsync(stoppingToken);

                    using var reader = new StreamReader(server);
                    var message = await reader.ReadLineAsync();

                    if (!string.IsNullOrEmpty(message) && message.StartsWith("CANCEL:"))
                    {
                        if (int.TryParse(message.Split(':')[1], out int jobId))
                        {
                            // Trigger the callback to the Worker
                            _onCancelRequested?.Invoke(jobId);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    // Log error or ignore transient pipe issues
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        public async Task StartAsyncOld(CancellationToken token)
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
