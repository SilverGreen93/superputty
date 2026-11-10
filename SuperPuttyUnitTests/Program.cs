using System;
using log4net;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// Shared support for the fixtures.  Used to be the entry point of a WinForms
    /// harness that launched the NUnit 2 gui runner; the tests are run by the test
    /// host now, so only the logging bootstrap is left.
    /// </summary>
    static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        static bool initialized = false;

        public static void InitLoggingForUnitTests()
        {
            lock (Log)
            {
                if (!initialized)
                {
                    log4net.Config.BasicConfigurator.Configure();
                    initialized = true;
                }
            }
        }
    }
}
