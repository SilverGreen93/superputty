using System;
using SuperPutty.Data;
using log4net;
using DarkModeForms;
using System.Windows.Forms;

namespace SuperPutty.Utils
{
    /// <summary>
    /// Helper class to parse host connection strings (e.g. ssh://localhost:2222)
    /// </summary>
    public class HostConnectionString
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HostConnectionString));

        public HostConnectionString(string hostString)
        {
            int idx = hostString.IndexOf("://");
            string hostPort = hostString;
            if (idx != -1)
            {
                // ssh://localhost:2020, or ssh2://localhost:2020
                if (Enum.TryParse(hostString.Substring(0, idx), true, out ConnectionProtocol protocol))
                {
                    Protocol = protocol;
                }
                else
                {
                    Messenger.MessageBox("Unknown protocol: " + hostString.Substring(0, idx) + ". Will default to ssh.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Protocol = null;
                }
                hostPort = hostString.Substring(idx + 3);
            }

            // Firefox addes a '/'...
            hostPort = hostPort.TrimEnd('/');
            int idxPort = hostPort.IndexOf(":");
            if (idxPort != -1)
            {
                // localhost:2020
                if (int.TryParse(hostPort.Substring(idxPort + 1), out int port))
                {
                    Host = hostPort.Substring(0, idxPort);
                    Port = port;
                }
                else
                {
                    Host = hostPort;
                }

            }
            else
            {
                // Try port separated by space
                idxPort = hostPort.IndexOf(" ");
                if (idxPort != -1)
                {
                    // localhost 2020
                    if (int.TryParse(hostPort.Substring(idxPort + 1), out int port))
                    {
                        Host = hostPort.Substring(0, idxPort);
                        Port = port;
                    }
                    else
                    {
                        Host = hostPort;
                    }

                }
                else
                {
                    // localhost
                    Host = hostPort;
                }
            }


            log.InfoFormat(
                "Parsed[{0}]: proto={1}, host={2}, port={3}", 
                hostString, 
                this.Protocol.HasValue ? this.Protocol.ToString() : "", 
                this.Host, 
                this.Port.HasValue ? this.Port.ToString() : "");
        }

        public ConnectionProtocol? Protocol { get; set; }
        public string Host { get; set; }
        public int? Port { get; set; }
    }
}
