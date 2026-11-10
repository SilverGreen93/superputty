using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SuperPutty.Data;
using log4net;

namespace SuperPutty.Utils
{
    public class SingleInstanceHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SingleInstanceHelper));

        /* Named pipes live in a machine wide namespace, so a fixed name is shared by
         * every terminal services session on the box.  Scope it to user and session so
         * that instances in different sessions neither talk to each other nor fight
         * over the pipe. */
        public static readonly string PipeName = GetPipeName();

        /// <summary>Upper bound on the number of arguments accepted from a client</summary>
        private const int MaxArgs = 1000;

        private static string GetPipeName()
        {
            string scope = string.Format("{0}_{1}_{2}",
                Environment.UserDomainName, Environment.UserName, Process.GetCurrentProcess().SessionId);
            StringBuilder name = new StringBuilder("SuperXPuTTYPipe_");
            foreach (char c in scope)
            {
                name.Append(char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.' ? c : '_');
            }
            return name.ToString();
        }

        // Start pipe server for single instance
        public static void StartPipeServer(Action<string[]> onArgsReceived)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        HandleOneConnection(onArgsReceived);
                    }
                    catch (Exception ex)
                    {
                        /* A broken client, a truncated message or a callback that throws
                         * must not end the listener, otherwise every later "open in
                         * existing instance" is silently lost. */
                        Log.Warn("Error while handling single instance pipe connection", ex);
                        Thread.Sleep(1000);
                    }
                }
            });
        }

        /// <summary>
        /// Waits for one client, hands over its arguments and closes the pipe.
        /// A new server stream is created per connection: disposing the reader used to
        /// dispose the pipe as well, so the following Disconnect() threw
        /// ObjectDisposedException and the listener died after the first launch.
        /// </summary>
        private static void HandleOneConnection(Action<string[]> onArgsReceived)
        {
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
            {
                server.WaitForConnection();

                // leaveOpen: the using block over the server closes the pipe, not the reader
                using (var reader = new BinaryReader(server, Encoding.UTF8, true))
                {
                    string[] args = ReadArgs(reader);
                    if (args != null)
                    {
                        Log.InfoFormat("Received remote Run command: [{0}]", string.Join(" ", args));
                        onArgsReceived?.Invoke(args);
                    }
                }
            }
        }

        /// <summary>Read the length prefixed arguments sent by <see cref="SendArgsToExistingInstance"/></summary>
        private static string[] ReadArgs(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            if (count <= 0 || count > MaxArgs)
            {
                Log.WarnFormat("Ignoring single instance request, invalid argument count: {0}", count);
                return null;
            }

            string[] args = new string[count];
            for (int i = 0; i < count; i++)
            {
                args[i] = reader.ReadString();
            }
            return args;
        }

        // Send args to existing instance
        public static bool SendArgsToExistingInstance(string[] args)
        {
            if (args == null)
            {
                args = Array.Empty<string>();
            }

            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                {
                    client.Connect(1000); // Wait up to 1 second
                    // leaveOpen so the client closes the pipe, not the writer
                    using (var writer = new BinaryWriter(client, Encoding.UTF8, true))
                    {
                        /* Length prefixed rather than tab joined and newline terminated:
                         * an argument may contain tabs, quotes or newlines. */
                        writer.Write(args.Length);
                        foreach (string arg in args)
                        {
                            writer.Write(arg ?? string.Empty);
                        }
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

        // Pipe-server callback registered by SuperPuTTY.Initialize: opens the session
        // described by the command line that a second instance forwarded to us.
        public static void Run(string[] args)
        {
            Log.InfoFormat("Received remote Run command: [{0}]", string.Join(" ", args));
            CommandLineOptions cmd = new CommandLineOptions(args);
            SessionDataStartInfo ssi = cmd.ToSessionStartInfo();
            SuperPuTTY.OpenSession(ssi);
        }
    }
}
