using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerTimeSync
{
	public class ServerConnection
	{
        public const uint DefaultPort = 4781;
        private AsynchronousSocketListener _asynchronousSocketListener;
	    private Thread _serverThread;
        private ManualResetEvent SendJob = new ManualResetEvent(false);
	    public EventHandler<Socket> OnConnect;
	    public EventHandler<StateObject> OnReceive;
	    public EventHandler<int> OnSend;
	    public ServerConnection ():this(DefaultPort)
		{
		}

	    public ServerConnection(uint defaultPort, IPAddress ipAddress)
	    {
            _asynchronousSocketListener = new AsynchronousSocketListener(defaultPort, ipAddress);
            ListnerEvents();
	    }

        public ServerConnection(uint defaultPort)
            : this(defaultPort, IPAddress.Parse("0.0.0.0"))
	    {
	    }

	    private void ListnerEvents()
	    {
	        _asynchronousSocketListener.OnConnect += OnConnectEvent;
	        _asynchronousSocketListener.OnReceive += OnReceiveEvent;
	        _asynchronousSocketListener.OnSend += OnSendEvent;
	    }

	    private void OnSendEvent(object sender, int BytesReceived)
	    {
            if(OnSend != null)
                OnSend(sender,BytesReceived);
	    }

	    private void OnReceiveEvent(object sender, StateObject e)
	    {
            if(OnReceive != null)
               OnReceive(sender,e);
	    }

	    private void OnConnectEvent(object sender, Socket e)
	    {
            if(OnConnect != null)
                OnConnect(sender,e);
	    }

	    public void Start()
	    {
	        ListenerFunctionThread();
	    }
	    public void StartThreaded()
	    {
	        _serverThread = new Thread(ListenerFunctionThread);
            _serverThread.Start();
	    }
	    private void ListenerFunctionThread()
	    {
	        try
	        {
                _asynchronousSocketListener.StartListening();
	        }
            catch (System.Net.Sockets.SocketException)
	        {
                throw new Exception("Port Alread Is Open");
	        }
	        
	    }

	    public void Send(Socket handler, string messsage)
	    {
            /*
	        var thread = new Thread(() => _asynchronousSocketListener.Send(handler, messsage));
            thread.Start();
            /*/
            _asynchronousSocketListener.Send(handler, messsage);
            //*/
	    }

	    public void Stop()
	    {
	        _asynchronousSocketListener.Dispose();
            if ((_serverThread != null) && _serverThread.IsAlive)
	        {
//                _serverThread.Abort();
                _serverThread.Join();
	        }
	            
	    }
	}
}

