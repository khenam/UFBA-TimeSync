using System;
using System.Net.Sockets;
using System.Threading;
using ServerTimeSync;

namespace ClientTimeSync
{
	public class ClientConnection
	{
        public EventHandler<Socket> OnConnect;
        public EventHandler<StateObject> OnReceive;
        public EventHandler<int> OnSend;
	    private AsynchronousClient _asynchronousClient;
	    private Thread _ClientThread;

        public ClientConnection(string hostName, uint port = ServerConnection.DefaultPort)
		{
            _asynchronousClient = new AsynchronousClient(hostName, (int) port);
            ListnerEvents();
		}

        private void ListnerEvents()
        {
            _asynchronousClient.OnConnect += OnConnectEvent;
            _asynchronousClient.OnReceive += OnReceiveEvent;
            _asynchronousClient.OnSend += OnSendEvent;
        }

        private void OnSendEvent(object sender, int BytesReceived)
        {
            if (OnSend != null)
                OnSend(sender, BytesReceived);
        }

        private void OnReceiveEvent(object sender, StateObject e)
        {
            if (OnReceive != null)
                OnReceive(sender, e);
        }

        private void OnConnectEvent(object sender, Socket e)
        {
            if (OnConnect != null)
                OnConnect(sender, e);
        }
        public void Connect()
        {
            ConnectFunctionThread();
        }
        public void ConnectThreaded()
        {
            _ClientThread = new Thread(ConnectFunctionThread);
            _ClientThread.Start();
        }

        private void ConnectFunctionThread()
        {
            try
            {
                _asynchronousClient.ConnectClient();
            }
            catch (System.Net.Sockets.SocketException)
            {
                throw new Exception("Port Alread Is Open");
            }

        }

	    public void Send(string message)
	    {
            _asynchronousClient.Send(message);
	    }

	    public void Stop()
	    {
	        _asynchronousClient.Dispose();
            if ((_ClientThread != null) && _ClientThread.IsAlive)
	        {
//	            _ClientThread.Abort();
	            _ClientThread.Join();
	        }
	    }
	}
}

