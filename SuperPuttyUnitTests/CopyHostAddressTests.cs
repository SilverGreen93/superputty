using NUnit.Framework;
using SuperPutty.Data;
using SuperPutty.Utils;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// "Copy host address" puts the address on the clipboard and nothing beside it, "Copy host address
    /// and port" adds the port the way the connection toolbar takes an address - which is what
    /// HostConnectionString reads back there.
    /// </summary>
    [TestFixture]
    public class CopyHostAddressTests
    {
        [TestCase("qnap", "192.168.0.2", 22, "192.168.0.2", "192.168.0.2 22")]
        [TestCase("jumphost", "10.20.30.40", 2222, "10.20.30.40", "10.20.30.40 2222")]
        [TestCase("gateway", "example.com", 0, "example.com", "example.com")]   // no port, nothing to add
        [TestCase("console", null, 22, "", "")]                                 // no host, no address
        public void TheAddressIsOneThingAndTheAddressWithItsPortAnother(
            string name, string host, int port, string address, string addressAndPort)
        {
            SessionData session = Session(name, host, port);
            Assert.That(session.Host ?? "", Is.EqualTo(address), name + " address");
            Assert.That(session.HostAndPort(), Is.EqualTo(addressAndPort), name + " address and port");
        }

        [TestCase("192.168.0.2", 22)]
        [TestCase("10.20.30.40", 2222)]
        [TestCase("example.com", 0)]
        public void WhatIsCopiedIsWhatTheConnectionToolbarReadsBack(string host, int port)
        {
            HostConnectionString parsed = new HostConnectionString(Session("s", host, port).HostAndPort());
            Assert.That(parsed.Host, Is.EqualTo(host), host + " address");
            Assert.That(parsed.Port ?? 0, Is.EqualTo(port), host + " port");
        }

        static SessionData Session(string name, string host, int port)
        {
            return new SessionData { SessionName = name, Host = host, Port = port, Proto = ConnectionProtocol.SSH };
        }
    }
}
