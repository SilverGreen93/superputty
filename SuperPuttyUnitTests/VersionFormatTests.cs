using System;
using System.Linq;
using NUnit.Framework;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// The version shown in the about box and the status bar drops the trailing zero
    /// components, so the assembly version 3.0.0.0 is displayed as 3.0.
    /// </summary>
    [TestFixture]
    public class VersionFormatTests
    {
        [TestCase("3.0.0.0", "3.0")]
        [TestCase("3.0.0", "3.0")]
        [TestCase("3.0", "3.0")]
        [TestCase("3.1.0.0", "3.1")]
        [TestCase("2.1.1.0", "2.1.1")]
        [TestCase("3.0.1.0", "3.0.1")]
        [TestCase("3.0.1.2", "3.0.1.2")]
        [TestCase("0.0.0.0", "0.0")]
        public void TrailingZerosAreNotShown(string version, string expected)
        {
            Assert.That(SuperPutty.SuperPuTTY.FormatVersion(new Version(version)), Is.EqualTo(expected));
        }

        [Test]
        public void TheDisplayedVersionOnlyDropsTrailingZerosFromTheAssemblyVersion()
        {
            // Version/DisplayVersion read the SuperXPuTTY assembly, not the test assembly
            string full = SuperPutty.SuperPuTTY.Version;
            string shown = SuperPutty.SuperPuTTY.DisplayVersion;
            Assert.That(full, Does.StartWith(shown), full + " should start with " + shown);
            string dropped = full.Length > shown.Length ? full.Substring(shown.Length + 1) : "";
            Assert.That(dropped.Split('.', StringSplitOptions.RemoveEmptyEntries).All(part => part == "0"), dropped);
        }
    }
}
