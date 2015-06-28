using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ClientTimeSync;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using ServerTimeSync;
using StateObject = ServerTimeSync.StateObject;

namespace TestTimeSync
{
	[TestFixture]
	public class TestServerClientTimeSync
	{
	    private const int DefaultPort = 4781;
	    private const int DefaultTimeout = 5000;
	    private const string TestString = "Test";
	    bool exceptionWasThrown = false;
	    private ServerConnection _server1;
	    private ClientConnection _client1;

// perform the test here, using whatever synchronization mechanisms needed
// to wait for threads to finish

// ...and detach the event handler

	    [SetUp]
		public void SetUp ()
	    {
            _server1 = new ServerConnection(DefaultPort,IPAddress.Parse("127.0.0.1"));
            _server1.StartThreaded();
	        _client1 = new ClientConnection("127.0.0.1");
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
	}
}

