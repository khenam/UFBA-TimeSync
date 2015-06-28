using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using TimeSyncBase;
using TimeSyncBase.Connection;
using TimeSyncBase.messages;
using TimeSyncBase.messages.requests;
using TimeSyncBase.messages.responses;

namespace ServerTimeSync
{
	public class ServerConnection : ConnectionBase
	{
        public const uint DefaultPort = 4781;
        
        private AsynchronousSocketListener _asynchronousSocketListener;
	    private Thread _serverThread;
        private ManualResetEvent SendJob = new ManualResetEvent(false);
        private LocalTime _localTime;
        
        public EventHandler<Socket> OnConnect;
	    public EventHandler<StateObject> OnReceive;
	    public EventHandler<int> OnSend;
	    private List<Socket> _socketList;

	    public ServerConnection ():this(DefaultPort)
		{
		}

	    public ServerConnection(uint defaultPort, IPAddress ipAddress, LocalTime localTime)
	    {
            _asynchronousSocketListener = new AsynchronousSocketListener(defaultPort, ipAddress);
	        _localTime = localTime;
            _socketList = new List<Socket>();
            ListnerEvents();
	    }

        public ServerConnection(uint defaultPort)
            : this(defaultPort, IPAddress.Parse("0.0.0.0"))
	    {
	    }

	    private ServerConnection(uint defaultPort, IPAddress ipAddress):this(defaultPort,ipAddress,new LocalTime())
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
                new Thread(() => OnSend(sender, BytesReceived)).Start();
	    }

	    private void OnReceiveEvent(object sender, StateObject e)
	    {
	        if (!TryReplyKnownProtocol(e) && OnReceive != null)
                new Thread(() => OnReceive(sender, e)).Start();
	    }

        protected override void HandleCorrectResponse(StateObject so, TimeSyncMessage message)
	    {
	        if (message is TimeSyncRequest)
	            Send(so.workSocket, buildTimeSyncResponse(message as TimeSyncRequest, so.receiveTime));
	        else if (message is TimeSyncConnectedClientsRequest)
	            Send(so.workSocket, buildTimeSyncConnectedClientsResponse());
	    }

	    private string buildTimeSyncConnectedClientsResponse()
	    {
	        var IpsList = new List<string>();
	        foreach (Socket socket in _socketList)
	        {
	            IPEndPoint ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint != null)
	                IpsList.Add(ipEndPoint.Address.ToString());
	        }
            var Message = new TimeSyncConnectedClientsResponse();
	        Message.ClientsIps = IpsList.ToArray();
	        return Message.ToJSON();
	    }

	    private string buildTimeSyncResponse(TimeSyncRequest message, DateTime receiveTime)
	    {
            var response = new TimeSyncResponse();
	        response.RequestTime = message.RequestTime;
            response.ReceivedTime = receiveTime.Add(_localTime.GetTimeSpan());
            response.ResponseTime = _localTime.GetDateTime();
	        return response.ToJSON();
	    }

	    private void OnConnectEvent(object sender, Socket e)
	    {
	        RegisterSocket(e);
            if(OnConnect != null)
                new Thread(()=>OnConnect(sender,e)).Start();
	    }

	    private void RegisterSocket(Socket socket)
	    {
	        _socketList.Add(socket);
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

