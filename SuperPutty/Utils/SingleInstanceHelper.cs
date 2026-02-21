using System;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;
using SuperPutty.Data;
using log4net;
using System.IO;

namespace SuperPutty.Utils
{
    public class SingleInstanceHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleInstanceHelper));
        public const string PipeName = "SuperXPuTTYPipe";

        // Start pipe server for single instance
        public static void StartPipeServer(Action<string[]> onArgsReceived)
        {
            Task.Run(() =>
            {
                using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                {
                    while (true)
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server, Encoding.UTF8))
                        {
                            string argsLine = reader.ReadLine();
                            if (!string.IsNullOrEmpty(argsLine))
                            {
                                string[] args = argsLine.Split('\t');
                                Log.InfoFormat("Received remote Run command: [{0}]", string.Join(" ", args));
                                onArgsReceived?.Invoke(args);
                            }
                        }
                        server.Disconnect();
                    }
                }
            });
        }

        // Send args to existing instance
        public static bool SendArgsToExistingInstance(string[] args)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000); // Wait up to 1 second
                    using (var writer = new StreamWriter(client, Encoding.UTF8))
                    {
                        writer.WriteLine(string.Join("\t", args));
                        writer.Flush();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn("Unable to send args to existing instance (maybe not running)", ex);
                return false;
            }
        }

        // Example usage: call this in Program/Main
        public static void Run(string[] args)
        {
            Log.InfoFormat("Received remote Run command: [{0}]", string.Join(" ", args));
            CommandLineOptions cmd = new CommandLineOptions(args);
            SessionDataStartInfo ssi = cmd.ToSessionStartInfo();
            SuperPuTTY.OpenSession(ssi);
        }
    }
}
