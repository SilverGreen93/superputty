using System.Collections.Generic;
using NUnit.Framework;
using System.Diagnostics;
using log4net;
using System.Threading;
using System.Configuration;
using SuperPutty.Data;
using SuperPutty.Scp;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;

namespace SuperPuttyUnitTests.Scp
{
    /// <summary>
    /// Basic SCP tests to test underlying process invocation limitations/quirks
    /// </summary>
    [TestFixture]
    class ScpTests
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScpTests));

        static ScpTests()
        {
            Program.InitLoggingForUnitTests();
        }

        Process proc;
        List<string> outData;
        List<string> errData;
        int lineNum = 0;

        [SetUp]
        public void Setup()
        {
            Log.InfoFormat("Setup");

            string unavailable = ScpConfig.NetworkTestUnavailableReason();
            if (unavailable != null)
            {
                Assert.Ignore(unavailable);
            }

            if (proc != null)
            {
                try
                {
                    proc.Refresh();
                    if (!proc.HasExited)
                    {
                        proc.Kill();
                    }
                }
                catch(Exception ex)
                {
                    Log.Warn("Error in Setup", ex);
                }
            }
            proc = new Process();

            lineNum = 0;
            outData = new List<string>();
            errData = new List<string>();
        }

        void proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.InfoFormat("ERR {0:000} {1}", ++lineNum, e.Data);
            this.errData.Add(e.Data);
        }

        void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Log.InfoFormat("OUT {0:000} {1}", ++lineNum, e.Data);
            this.outData.Add(e.Data);
        }

        void ListDirectory(string user, string password, string host)
        {
            string args = string.Format(
                "-ls -load \"Default Settings\" -P 22 -pw {0} {1}@{2}:.",
                password, user, host);
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = ScpConfig.PscpLocation,
                Arguments = args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            proc.StartInfo = startInfo;
            proc.EnableRaisingEvents = true;

            Log.InfoFormat("Starting list: args={0}", args);
            if (proc.Start())
            {
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.ErrorDataReceived += proc_ErrorDataReceived;

                Log.Info("BeginErrorReadLine()");
                proc.BeginErrorReadLine();
                Log.Info("BeginOutputReadLine()");
                proc.BeginOutputReadLine();
                Log.Info("WaitForExit(5000)");
                proc.WaitForExit(5000);

                proc.CancelErrorRead();
                proc.CancelOutputRead();
                Log.InfoFormat("Done");
            }
            else
            {
                Assert.Fail("Process failed to start");
            }
        }


        /// <summary>
        /// Simple test to run scp and get a listing back.  
        /// </summary>
        [Test, Category("NetworkTest")]
        public void ListDirSuccess()
        {
            ListDirectory(ScpConfig.UserName, ScpConfig.Password, ScpConfig.KnownHost);
            Assert.Greater(this.outData.Count, 0);
        }

        /// <summary>
        /// A bad password used to leave pscp sitting at a "user@host's password:" prompt, which
        /// blocked the process.  pscp refuses to prompt when -pw was used and now exits with
        /// "FATAL ERROR: Configured password was not accepted" instead.
        /// </summary>
        [Test, Category("NetworkTest")]
        public void ListDirBadPassword()
        {
            bool returned = false;
            Exception error = null;
            Thread t = new Thread(x => 
            {
                try
                {
                    ListDirectory(ScpConfig.UserName, ScpConfig.Password + "zzz", ScpConfig.KnownHost);
                }
                catch (Exception ex)
                {
                    // must not escape: an unhandled exception on this thread takes down
                    // the whole test host process
                    error = ex;
                }
                lock (this)
                {
                    Monitor.Pulse(this);
                    returned = true;
                }
            });
            t.IsBackground = true;
            t.Start();

            lock (this)
            {
                Monitor.Wait(this, 5000);
            }
            if (error != null)
            {
                Assert.Fail("ListDirectory failed: {0}", error.GetBaseException().Message);
            }
            Assert.True(returned, "pscp exits instead of hanging when -pw is refused");
            Assert.AreEqual(0, this.outData.Count(line => !string.IsNullOrEmpty(line)));
            StringAssert.Contains("not accepted", string.Join(" ", this.errData));
        }

        /// <summary>
        /// An uncached host key makes pscp wait for an interactive "Store key in cache?" answer, so
        /// it never returns.  Current pscp writes that prompt to the console rather than to its
        /// redirected stderr, so nothing arrives on either stream; PscpClient has to time out and
        /// kill it, which PscpClientListDirTests.ListDirHostNoKey covers.
        /// </summary>
        [Test, Category("NetworkTest")]
        public void ListDirHostNoKey()
        {
            bool returned = false;
            Exception error = null;
            Thread t = new Thread(x =>
            {
                try
                {
                    ListDirectory(ScpConfig.UserName, ScpConfig.Password, ScpConfig.UnKnownHost);
                }
                catch (Exception ex)
                {
                    // must not escape: an unhandled exception on this thread takes down
                    // the whole test host process
                    error = ex;
                }
                lock (this)
                {
                    Monitor.Pulse(this);
                    returned = true;
                }
            });
            t.IsBackground = true;
            t.Start();

            lock (this)
            {
                Monitor.Wait(this, 5000);
            }
            if (error != null)
            {
                Assert.Fail("ListDirectory failed: {0}", error.GetBaseException().Message);
            }
            Assert.False(returned, "pscp blocks until the host key question is answered");
            Assert.AreEqual(0, this.outData.Count);
            Assert.AreEqual(0, this.errData.Count);
        }
    }

    [TestFixture]
    public class PscpClientListDirTests 
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PscpClientListDirTests));

        [SetUp]
        public void CheckTestEnvironment()
        {
            string unavailable = ScpConfig.NetworkTestUnavailableReason();
            if (unavailable != null)
            {
                Assert.Ignore(unavailable);
            }
        }

        [Test, Category("NetworkTest")]
        public void ListDirSuccess()
        {
            SessionData session = new SessionData
            {
                Username = ScpConfig.UserName, 
                Password = ScpConfig.Password, 
                Host = ScpConfig.KnownHost, 
                Port = 22
            };

            PscpClient client = new PscpClient(ScpConfig.DefaultOptions, session);

            ListDirectoryResult res = client.ListDirectory(new BrowserFileInfo { Path = "." });

            Assert.AreEqual(ResultStatusCode.Success, res.StatusCode);
            Assert.Greater(res.Files.Count, 0);
            foreach (BrowserFileInfo file in res.Files)
            {
                Log.Info(file);
            }

            Log.InfoFormat("Result: {0}", res);
        }

        [Test, Category("NetworkTest")]
        public void ListDirBadPassword()
        {
            SessionData session = new SessionData
            {
                Username = ScpConfig.UserName,
                Password = ScpConfig.Password + "xxx",
                Host = ScpConfig.KnownHost,
                Port = 22
            };

            PscpClient client = new PscpClient(ScpConfig.DefaultOptions, session);

            ListDirectoryResult res = client.ListDirectory(new BrowserFileInfo { Path = "." });

            // pscp only reaches RetryAuthentication by prompting on stdout, and it refuses to
            // prompt when -pw was sent: it exits with "Configured password was not accepted",
            // which PscpClient reports through its generic error path
            Assert.AreEqual(ResultStatusCode.Error, res.StatusCode);
            Assert.AreEqual(0, res.Files.Count);
            StringAssert.Contains("not accepted", res.ErrorMsg);

            Log.InfoFormat("Result: {0}", res);
        }

        [Test, Category("NetworkTest")]
        public void ListDirHostNoKey()
        {
            SessionData session = new SessionData
            {
                Username = ScpConfig.UserName,
                Password = ScpConfig.Password,
                Host = ScpConfig.UnKnownHost,
                Port = 22
            };

            PscpClient client = new PscpClient(ScpConfig.DefaultOptions, session);

            ListDirectoryResult res = client.ListDirectory(new BrowserFileInfo { Path = "." });

            Assert.AreEqual(ResultStatusCode.Error, res.StatusCode);
            Assert.AreEqual(res.Files.Count, 0);

            Log.InfoFormat("Result: {0}", res);
        }

        [Test, Category("NetworkTest")]
        public void ListDirBadPath()
        {
            SessionData session = new SessionData
            {
                Username = ScpConfig.UserName,
                Password = ScpConfig.Password,
                Host = ScpConfig.KnownHost,
                Port = 22
            };

            PscpClient client = new PscpClient(ScpConfig.DefaultOptions, session);

            ListDirectoryResult res = client.ListDirectory(new BrowserFileInfo { Path = "some_non_existant_dir" });

            Assert.AreEqual(ResultStatusCode.Error, res.StatusCode);
            Assert.AreEqual(0, res.Files.Count);
            // pscp puts the bad path message on stderr and exits non-zero, so PscpClient keeps it
            // inside its "Unknown error.  exitCode=..." text rather than returning it directly
            StringAssert.Contains("Unable to open", res.ErrorMsg);
            Log.InfoFormat("Result: {0}", res);
        }
    }

    [TestFixture]
    public class PscpClientTransferTests
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PscpClientTransferTests));

        [SetUp]
        public void CheckTestEnvironment()
        {
            string unavailable = ScpConfig.NetworkTestUnavailableReason();
            if (unavailable != null)
            {
                Assert.Ignore(unavailable);
            }
        }

        static PscpClientTransferTests()
        {
            Program.InitLoggingForUnitTests();
        }

        [Test, Category("NetworkTest")]
        public void LocalToRemote()
        {
            SessionData session = new SessionData
            {
                Username = ScpConfig.UserName,
                Password = ScpConfig.Password,
                Host = ScpConfig.KnownHost,
                Port = 22
            };

            List<BrowserFileInfo> sourceFiles = new List<BrowserFileInfo> 
            {
                //new BrowserFileInfo(new FileInfo(Path.GetTempFileName()))
                new BrowserFileInfo(new FileInfo(ScpConfig.PscpLocation))
            };
            BrowserFileInfo target = new BrowserFileInfo
            { 
                Path = string.Format("/home/{0}/", session.Username), 
                Source = SourceType.Remote
            };

            PscpOptions options = new PscpOptions { PscpLocation = ScpConfig.PscpLocation, TimeoutMs = 5000 };
            PscpClient client = new PscpClient(options, session);
            PscpResult res = client.CopyFiles(
                sourceFiles, 
                target, 
                (complete, cancelAll, status) => 
                {
                    Log.InfoFormat(
                        "complete={0}, cancelAll={1}, fileName={2}, pctComplete={3}", 
                        complete, cancelAll, status.Filename, status.PercentComplete);
                });

            Log.InfoFormat("Result: {0}", res);

            /*
            ListDirectoryResult res = client.ListDirectory(new BrowserFileInfo { Path = "." });

            Assert.AreEqual(ResultStatusCode.Success, res.StatusCode);
            Assert.Greater(res.Files.Count, 0);
            foreach (BrowserFileInfo file in res.Files)
            {
                Log.Info(file);
            }

            Log.InfoFormat("Result: {0}", res);*/
        }
    }

    /// <summary>
    /// Settings for the pscp based tests, from the SuperPuTTY.ScpTests.* keys in app.config
    /// </summary>
    public class ScpConfig
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ScpConfig));

        public static readonly string PscpLocation;
        public static readonly string UserName;
        public static readonly string Password;
        public static readonly string KnownHost;
        public static readonly string UnKnownHost;

        public static readonly PscpOptions DefaultOptions;

        static ScpConfig()
        {
            NameValueCollection settings = LoadSettings();
            PscpLocation = ResolvePscpLocation(settings["SuperPuTTY.ScpTests.PscpLocation"]);
            UserName = settings["SuperPuTTY.ScpTests.UserName"];
            Password = settings["SuperPuTTY.ScpTests.Password"];
            KnownHost = settings["SuperPuTTY.ScpTests.KnownHost"];
            UnKnownHost = settings["SuperPuTTY.ScpTests.UnKnownHost"];
            DefaultOptions = new PscpOptions { PscpLocation = PscpLocation };
            Log.InfoFormat("PscpLocation={0}, KnownHost={1}", PscpLocation, KnownHost);
        }

        /// <summary>
        /// The tests run inside the VSTest testhost process, which does not load the .config
        /// file of the assembly under test, so open SuperPuttyUnitTests.dll.config directly.
        /// </summary>
        private static NameValueCollection LoadSettings()
        {
            try
            {
                string configFile = Assembly.GetExecutingAssembly().Location + ".config";
                if (File.Exists(configFile))
                {
                    Configuration config = ConfigurationManager.OpenMappedExeConfiguration(
                        new ExeConfigurationFileMap { ExeConfigFilename = configFile },
                        ConfigurationUserLevel.None);
                    NameValueCollection settings = new NameValueCollection();
                    foreach (string key in config.AppSettings.Settings.AllKeys)
                    {
                        settings[key] = config.AppSettings.Settings[key].Value;
                    }
                    return settings;
                }
                Log.WarnFormat("no test configuration file at {0}", configFile);
            }
            catch (Exception ex)
            {
                Log.Warn("Error reading test configuration", ex);
            }
            return ConfigurationManager.AppSettings;
        }

        /// <summary>
        /// Makes the configured SuperPuTTY.ScpTests.PscpLocation usable.  An absolute path is kept
        /// as it is; a relative one (the default points at the pscp.exe built from the putty
        /// submodule) is looked for against the directories above the test assembly, since VSTest
        /// starts the tests from an unpredictable working directory and the depth of the output
        /// folder depends on the platform and configuration.
        /// </summary>
        private static string ResolvePscpLocation(string configured)
        {
            if (string.IsNullOrEmpty(configured))
            {
                return configured;
            }
            if (Path.IsPathRooted(configured))
            {
                return configured;
            }

            string exe = Path.GetFileName(configured);
            string directory = AppContext.BaseDirectory;
            while (directory != null)
            {
                string candidate = Path.GetFullPath(Path.Combine(directory, configured));
                if (File.Exists(candidate))
                {
                    Log.InfoFormat("Resolved pscp to {0}", candidate);
                    return candidate;
                }

                // cmake with the Visual Studio generator puts pscp.exe under build\<config>\, with
                // Ninja directly under build\, so search the tree rather than guess the layout
                string build = Path.Combine(directory, "putty", "build");
                if (Directory.Exists(build))
                {
                    foreach (string found in Directory.GetFiles(build, exe, SearchOption.AllDirectories))
                    {
                        Log.InfoFormat("Resolved pscp to {0}", found);
                        return found;
                    }
                }

                DirectoryInfo parent = new DirectoryInfo(directory).Parent;
                directory = parent != null ? parent.FullName : null;
            }

            Log.WarnFormat("pscp not found for '{0}' configured in app.config", configured);
            return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, configured));
        }

        /// <summary>
        /// Why the pscp based tests cannot run here, or null when they can.  They need a real
        /// pscp.exe and a live ssh server, so on machines that have neither (build agents, most
        /// dev boxes) they are skipped rather than failed.
        /// </summary>
        public static string NetworkTestUnavailableReason()
        {
            if (string.IsNullOrEmpty(PscpLocation))
            {
                return "SuperPuTTY.ScpTests.PscpLocation is not set in app.config";
            }
            if (!File.Exists(PscpLocation))
            {
                return string.Format("pscp not found at '{0}'", PscpLocation);
            }
            if (string.IsNullOrEmpty(KnownHost))
            {
                return "SuperPuTTY.ScpTests.KnownHost is not set in app.config";
            }
            if (!IsPortOpen(KnownHost, 22))
            {
                return string.Format("nothing listening on {0}:22", KnownHost);
            }
            return null;
        }

        private static bool IsPortOpen(string host, int port)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    return client.ConnectAsync(host, port).Wait(3000) && client.Connected;
                }
            }
            catch (Exception ex)
            {
                Log.InfoFormat("{0}:{2} not reachable: {1}", host, ex.Message, port);
                return false;
            }
        }
    }
}
