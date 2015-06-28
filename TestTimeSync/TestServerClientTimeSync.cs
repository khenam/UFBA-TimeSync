using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClientTimeSync;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ServerTimeSync;
using TimeSyncBase;
using TimeSyncBase.Connection;

namespace TestTimeSync
{
	[TestFixture]
	public class TestServerClientTimeSync
	{
	    private const int DefaultPort = 4781;
	    private const int DefaultTimeout = 5000;
	    private const string TestString = "Test";
	    private const string LocalHostIP = "127.0.0.1";
	    bool exceptionWasThrown = false;
	    private ServerConnection _server1;
	    private ClientConnection _client1;
        private LocalTime _localTimeServer = new LocalTime();
        private LocalTime _localTimeClient = new LocalTime();

// perform the test here, using whatever synchronization mechanisms needed
// to wait for threads to finish

// ...and detach the event handler

	    [SetUp]
		public void SetUp ()
	    {
	        _server1 = new ServerConnection(DefaultPort,IPAddress.Parse("0.0.0.0"), _localTimeServer);
            _server1.StartThreaded();
            _client1 = new ClientConnection(LocalHostIP, _localTimeClient);
	    }

	    [TearDown]
	    public void TearDown()
	    {
            _server1.Stop();
            _client1.Stop();
	    }

	    [Test]
        [Timeout(DefaultTimeout)]
        public void ValidateConnectionEvent ()
	    {
	        var serverConnected = false;
	        var clientConnected = false;
            _server1.OnConnect += (sender, socket) => serverConnected = true;
            _client1.OnConnect += (sender, socket) => clientConnected = true;
            _client1.ConnectThreaded();
	        while (!serverConnected || !clientConnected) ;
            Assert.That(serverConnected, Is.True);
            Assert.That(clientConnected, Is.True);
        }
        [Test]
        [Timeout(DefaultTimeout)]
        public void ValidateReceivedAndSentEvent()
        {
            var serverReceive = false;
            var clientReceive = false;
            var serverSend = false;
            var clientSend = false;
            var clientConnected = false;
            _client1.OnConnect += (sender, socket) => clientConnected = true;
            _client1.OnReceive += (sender, o) => clientReceive = (o.sb.ToString() == TestString);
            _server1.OnReceive += delegate(object sender, StateObject o) { serverReceive = (o.sb.ToString() == TestString); _server1.Send(o.workSocket, TestString); };
            _server1.OnSend += (sender, i) => serverSend = (TestString.Length==i);
            _client1.OnSend += (sender, i) => clientSend = (TestString.Length == i);
            _client1.ConnectThreaded();
            while (!clientConnected) ;
            _client1.Send(TestString);
            while (!serverReceive || !clientReceive) ;
            Assert.That(serverReceive, Is.True);
            Assert.That(clientReceive, Is.True);
            while (!serverSend || !clientSend) ;
            Assert.That(serverSend, Is.True);
            Assert.That(clientSend, Is.True);
        }

	    [Test]
	    [Timeout(DefaultTimeout)]
	    public void InterpretTimeSyncMessageEvent()
	    {
	        var clientTimeSincHappened = false;
	        TimeSpan serverDifference = new TimeSpan(0, -16, 37);
	        _localTimeServer.SetDateTime(DateTime.Now.Add(serverDifference));
	        Assert.That(_localTimeServer.GetTimeSpan(), Is.EqualTo(serverDifference));
	        TimeSpan clientTimeSpan = _localTimeClient.GetTimeSpan();
	        Assert.That(serverDifference, Is.Not.EqualTo(clientTimeSpan));
	        _client1.OnTimeSync += (sender, dateTime) =>
	        {
	            clientTimeSincHappened = true;
                Assert.That(dateTime, Is.EqualTo(DateTime.Now.Add(serverDifference)));
	        };

	        _client1.ConnectThreaded();
	        _client1.SyncTime();
            while (clientTimeSincHappened == false) ;
            Assert.That(Math.Round(_client1.GetLocalTime().GetTimeSpan().TotalSeconds, 0), Is.EqualTo(serverDifference.TotalSeconds));
	    }

        [Test]
        [Timeout(DefaultTimeout)]
        public void InterpretTimeSyncConnectedClientsMessageEvent()
        {
            ManualResetEvent ListHappen = new ManualResetEvent(false);
            List<IPAddress> ipListReceived = new List<IPAddress>();
            var expected = 1;
            ListHappen.Reset();
            _client1.OnUpdateClientList += (sender, ipList) =>
            {
                ipListReceived = ipList;
                Assert.That(ipListReceived.Count, Is.EqualTo(expected));
                ListHappen.Set();
            };
            _client1.ConnectThreaded();
            _client1.FoundNewClients();
            ListHappen.WaitOne();
            Assert.That(string.Join(".", ipListReceived[0].GetAddressBytes().Select(a => a.ToString("d"))), Is.EqualTo(LocalHostIP));

            ListHappen.Reset();
            expected = 2;
            var clientConnected = false;
            var client2 = new ClientConnection(LocalHostIP);
            client2.OnConnect += (sender, socket) => clientConnected = true;
            client2.ConnectThreaded();
            while (!clientConnected) ;
            _client1.FoundNewClients();
            ListHappen.WaitOne();
            Assert.That(string.Join(".", ipListReceived[1].GetAddressBytes().Select(a => a.ToString("d"))), Is.EqualTo(LocalHostIP));
        }
	}
}

