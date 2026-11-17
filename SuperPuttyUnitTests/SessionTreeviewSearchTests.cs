using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SuperPutty;
using SuperPutty.Data;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// The box on top of the sessions panel filters the tree through SessionTreeview.SearchFilter,
    /// which looks at the name, the host and the port of every session.  Only the filter is used
    /// here, the panel itself never opens.
    /// </summary>
    [TestFixture]
    public class SessionTreeviewSearchTests
    {
        [TestCase("Ubuntu", "Ubuntu WSL")]
        [TestCase("172", "Ubuntu WSL")]
        [TestCase("192.168.0.2", "qnap", "qnap-1")]
        [TestCase("22", "Ubuntu WSL", "qnap", "qnap-1", "mintty", "jumphost")]  // four on 22, one on 2222
        [TestCase("2222", "jumphost")]                                // the port alone finds it
        [TestCase("222", "jumphost")]                                 // part of a port is enough
        [TestCase("4321")]                                            // no session on that port
        [TestCase("serial", "serial")]                                // no port, still answers to its name
        public void TheBoxFindsSessionsByNameHostAndPort(string typed, params string[] expectedNames)
        {
            Assert.That(Matches("CaseInSensitive", typed), Is.EquivalentTo(expectedNames), "typing " + typed);
        }

        [TestCase("wsl", "Ubuntu WSL")]                               // letters may be typed in any case
        [TestCase("172.22", "Ubuntu WSL")]
        [TestCase("2222", "jumphost")]
        public void TheCaseInSensitiveModeIgnoresTheCaseOfTheLetters(string typed, params string[] expectedNames)
        {
            Assert.That(Matches("CaseInSensitive", typed), Is.EquivalentTo(expectedNames), "typing " + typed);
        }

        [TestCase("WSL", "Ubuntu WSL")]
        [TestCase("wsl")]                                             // the name is written with capitals
        [TestCase("JUMPHOST")]
        [TestCase("2222", "jumphost")]                                // digits have no case
        public void TheCaseSensitiveModeWantsTheLettersAsTheyAre(string typed, params string[] expectedNames)
        {
            Assert.That(Matches("CaseSensitive", typed), Is.EquivalentTo(expectedNames), "typing " + typed);
        }

        [TestCase("WSL$", "Ubuntu WSL")]
        [TestCase("^172", "Ubuntu WSL")]                              // anchored on the host
        [TestCase(@"^192\.168\.0\.2$", "qnap", "qnap-1")]
        [TestCase(@"^22$", "Ubuntu WSL", "qnap", "qnap-1", "mintty")] // anchored on the port, 2222 is out
        [TestCase("^4321$")]
        [TestCase("^(qnap|jumphost)", "qnap", "qnap-1", "jumphost")]
        public void TheRegexModeRunsOverTheNameHostAndPort(string pattern, params string[] expectedNames)
        {
            Assert.That(Matches("Regex", pattern), Is.EquivalentTo(expectedNames), "pattern " + pattern);
        }

        [Test]
        public void ASessionWithoutAHostIsStillCheckedAgainstItsNameAndPort()
        {
            // Host is null on a session that never got one, which used to throw
            SessionData noHost = Session("console", null, 0, ConnectionProtocol.Serial);
            Assert.That(new SessionTreeview.SearchFilter("CaseInSensitive", "console").IsMatch(noHost), Is.True);
            Assert.That(new SessionTreeview.SearchFilter("CaseSensitive", "console").IsMatch(noHost), Is.True);
            Assert.That(new SessionTreeview.SearchFilter("Regex", "console").IsMatch(noHost), Is.True);

            Assert.That(new SessionTreeview.SearchFilter("CaseInSensitive", "22").IsMatch(noHost), Is.False);
        }

        [TestCase("CaseInSensitive")]
        [TestCase("CaseSensitive")]
        [TestCase("Regex")]
        public void ASessionWithNoPortIsNotFoundByAPort(string mode)
        {
            // Port is 0 until a session is given one, which is not the same as being on port "0"
            SessionData noPort = Session("gateway", "example.com", 0, ConnectionProtocol.SSH);
            Assert.That(new SessionTreeview.SearchFilter(mode, "0").IsMatch(noPort), Is.False, mode);
            Assert.That(new SessionTreeview.SearchFilter(mode, "example").IsMatch(noPort), Is.True, mode);
        }

        static string[] Matches(string mode, string filter)
        {
            SessionTreeview.SearchFilter search = new SessionTreeview.SearchFilter(mode, filter);
            return Sessions().Where(search.IsMatch).Select(s => s.SessionName).ToArray();
        }

        static List<SessionData> Sessions()
        {
            return new List<SessionData>
            {
                Session("Ubuntu WSL", "172.22.123.154", 22, ConnectionProtocol.SSH),
                Session("qnap", "192.168.0.2", 22, ConnectionProtocol.SSH),
                Session("qnap-1", "192.168.0.2", 22, ConnectionProtocol.SSH),
                Session("mintty", "localhost", 22, ConnectionProtocol.Mintty),
                Session("jumphost", "10.20.30.40", 2222, ConnectionProtocol.SSH),
                Session("serial", "COM1", 0, ConnectionProtocol.Serial),
            };
        }

        static SessionData Session(string name, string host, int port, ConnectionProtocol proto)
        {
            return new SessionData { SessionName = name, Host = host, Port = port, Proto = proto };
        }
    }
}
