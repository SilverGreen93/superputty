using System.Data;
using System.Linq;
using NUnit.Framework;
using SuperPutty.Gui;

namespace SuperPuttyUnitTests
{
    /// <summary>
    /// What is typed into the session box is turned into a DataView row filter, which searches the
    /// columns the list shows: the name, the id, the host and the port.  The list itself never opens
    /// here, only the filter is applied to the data.
    /// </summary>
    [TestFixture]
    public class QuickSelectorFilterTests
    {
        [TestCase("Ubuntu", "Ubuntu WSL")]
        [TestCase("ubuntu wsl", "Ubuntu WSL")]                   // split on the space, both parts in one row
        [TestCase("qnap", "qnap", "qnap-1")]
        [TestCase("mintty", "mintty")]
        [TestCase("172", "Ubuntu WSL")]
        [TestCase("172.22.123.154", "Ubuntu WSL")]               // split on the dots, all parts in one host
        [TestCase("192.168", "qnap", "qnap-1")]
        [TestCase("local", "mintty")]
        [TestCase("serial", "serial")]                           // no host, still answers to its name
        [TestCase("22", "Ubuntu WSL", "qnap", "qnap-1", "mintty")]   // every ssh session answers to its port
        [TestCase("4321", "serial")]                             // the port finds a session with no host
        [TestCase("43", "serial")]
        [TestCase("22 43")]                                      // both parts have to land in one row
        [TestCase("999")]                                        // nothing matches
        public void TypedTextIsSoughtInNameDetailHostAndPort(string typed, params string[] expectedNames)
        {
            Assert.That(Matches(typed), Is.EquivalentTo(expectedNames), "typing " + typed);
        }

        [Test]
        public void AnEmptyBoxLeavesTheListAlone()
        {
            // UpdateFilter clears the row filter for an empty box, and FormatFilterString agrees
            DataView view = new DataView(Sessions()) { RowFilter = QuickSelector.FormatFilterString("") };
            Assert.That(QuickSelector.FormatFilterString(""), Is.Empty);
            Assert.That(view.Count, Is.EqualTo(5));
        }

        [TestCase("Ubuntu WSL", "172.22.123.154", "22")]
        [TestCase("serial", "", "4321")]                         // a session without a host still has a port
        public void TheListCarriesTheHostAndThePortToDisplay(string name, string host, string port)
        {
            DataRow row = Sessions().Select().Single(r => (string) r["Name"] == name);
            Assert.That(row["Host"], Is.EqualTo(host), name + " host");
            Assert.That(row["Port"], Is.EqualTo(port), name + " port");
        }

        static string[] Matches(string typed)
        {
            DataView view = new DataView(Sessions()) { RowFilter = QuickSelector.FormatFilterString(typed) };
            return view.Cast<DataRowView>().Select(row => (string) row["Name"]).ToArray();
        }

        static QuickSelectorData.ItemDataDataTable Sessions()
        {
            // the rows the session lists build: name, id, colour, tag, host and port
            QuickSelectorData data = new QuickSelectorData();
            data.ItemData.AddItemDataRow("Ubuntu WSL", "Ubuntu WSL", null, null, "172.22.123.154", "22");
            data.ItemData.AddItemDataRow("qnap", "qnap", null, null, "192.168.0.2", "22");
            data.ItemData.AddItemDataRow("qnap-1", "qnap-1", null, null, "192.168.0.2", "22");
            data.ItemData.AddItemDataRow("mintty", "mintty", null, null, "localhost", "22");
            data.ItemData.AddItemDataRow("serial", "serial", null, null, "", "4321");
            return data.ItemData;
        }
    }
}
