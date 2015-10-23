using System;
using System.Linq;
using System.Net;
using System.Threading;
using ClientTimeSync;
using NUnit.Framework;
using ServerTimeSync;
using TimeSyncBase;

namespace TestTimeSync
{
    [TestFixture]
    public class TestServerClientTimeSync
    {
// perform the test here, using whatever synchronization mechanisms needed
// to wait for threads to finish

// ...and detach the event handler

        [SetUp]
        public void SetUp()
        {
            _server1 = new ServerConnection(DefaultPort, IPAddress.Parse("0.0.0.0"), _localTimeServer);
            _server1.StartThreaded();
            _client1 = new ClientConnection(LocalHostIP, PortClient1, _localTimeClient);
        }

        [TearDown]
        public void TearDown()
        {
            _client1.Stop();
            _server1.Stop();
        }

        private const int DefaultPort = 4781;
        private const int PortClient1 = 4782;
        private const int PortClient2 = 4783;
        private const int DefaultTimeout = 2000;
        private const string TestString = "Test";
        private const string LocalHostIP = "127.0.0.1";
        private ServerConnection _server1;
        private ClientConnection _client1;
        private readonly LocalTime _localTimeServer = new LocalTime();
        private readonly LocalTime _localTimeClient = new LocalTime();

        [Test]
//        [Timeout(DefaultTimeout)]
        public void InterpretTimeSyncConnectedClientsMessageEvent()
        {
            var ListHappen = new AutoResetEvent(false);
            var clientConnectedEvent = new AutoResetEvent(false);
            var expected = 1;
            _client1.OnUpdateClientList += (sender, ipList) =>
            {
                Assert.That(ipList.Count, Is.EqualTo(expected));
                Assert.That(string.Join(".", ipList[expected - 1].GetAddressBytes().Select(a => a.ToString("d"))),
                    Is.EqualTo(LocalHostIP));
                ListHappen.Set();
            };
            _client1.ConnectThreaded();
            _client1.FoundNewClients();
            Assert.That(ListHappen.WaitOne(DefaultTimeout), Is.True);
            expected = 2;
            var client2 = new ClientConnection(LocalHostIP, PortClient2);
            client2.OnConnect += (sender, socket) => clientConnectedEvent.Set();
            client2.OnUpdateClientList += _client1.OnUpdateClientList;
            client2.ConnectThreaded();
            clientConnectedEvent.WaitOne();
            client2.FoundNewClients();
            Assert.That(ListHappen.WaitOne(DefaultTimeout), Is.True);
            _client1.FoundNewClients();
            Assert.That(ListHappen.WaitOne(DefaultTimeout), Is.True);
        }

        [Test]
        [Timeout(DefaultTimeout)]
        public void InterpretTimeSyncMessageEvent()
        {
            var clientTimeSincHappened = false;
            var serverDifference = new TimeSpan(0, -16, 37);
            _localTimeServer.SetDateTime(DateTime.Now.Add(serverDifference));
            Assert.That(Math.Round(_localTimeServer.GetTimeSpan().TotalMilliseconds, 0),
                Is.EqualTo(serverDifference.TotalMilliseconds));
            var clientTimeSpan = _localTimeClient.GetTimeSpan();
            Assert.That(serverDifference, Is.Not.EqualTo(clientTimeSpan));
            _client1.OnTimeSync += (sender, dateTime) =>
            {
                clientTimeSincHappened = true;
                Assert.That(dateTime.ToString("yyyy-MM-dd hh:mm:ss"),
                    Is.EqualTo(DateTime.Now.Add(serverDifference).ToString("yyyy-MM-dd hh:mm:ss")));
            };

            _client1.ConnectThreaded();
            _client1.SyncTime();
            while (clientTimeSincHappened == false) ;
            Assert.That(Math.Round(_client1.GetLocalTime().GetTimeSpan().TotalSeconds, 0),
                Is.EqualTo(serverDifference.TotalSeconds));
        }

        [Test]
        [Timeout(DefaultTimeout)]
        public void ValidateConnectionEvent()
        {
            var serverConnected = new AutoResetEvent(false);
            var clientConnected = new AutoResetEvent(false);
            _server1.OnConnect += (sender, socket) => serverConnected.Set();
            _client1.OnConnect += (sender, socket) => clientConnected.Set();
            _client1.ConnectThreaded();
            Assert.That(serverConnected.WaitOne(DefaultTimeout), Is.True);
            Assert.That(clientConnected.WaitOne(DefaultTimeout), Is.True);
        }

        [Test]
//        [Timeout(DefaultTimeout)]
        public void ValidateReceivedAndSentEvent()
        {
            var clientConnected = new AutoResetEvent(false);
            var serverReceive = new AutoResetEvent(false);
            var clientReceive = new AutoResetEvent(false);
            var serverSend = new AutoResetEvent(false);
            var clientSend = new AutoResetEvent(false);

            _client1.OnConnect += (sender, socket) => clientConnected.Set();
            _client1.OnReceive += (sender, o) => { if (o.sb.ToString() == TestString) clientReceive.Set(); };
            _server1.OnReceive += (sender, o) =>
            {
                if (o.sb.ToString() == TestString) serverReceive.Set();
                _server1.Send(o.workSocket, TestString);
            };
            _server1.OnSend += (sender, i) => { if (TestString.Length == i) serverSend.Set(); };
            _client1.OnSend += (sender, i) => { if (TestString.Length == i) clientSend.Set(); };
            _client1.ConnectThreaded();
            Assert.That(clientConnected.WaitOne(DefaultTimeout), Is.True);
            _client1.Send(TestString);
            Assert.That(serverReceive.WaitOne(DefaultTimeout), Is.True);
            Assert.That(clientReceive.WaitOne(DefaultTimeout), Is.True);
            Assert.That(serverSend.WaitOne(DefaultTimeout), Is.True);
            Assert.That(clientSend.WaitOne(DefaultTimeout), Is.True);
        }
    }
}