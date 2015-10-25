using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        private readonly AsynchronousSocketListener _asynchronousSocketListener;
        private readonly LocalTime _localTime;
        private readonly List<Socket> _socketList;
        private Thread _serverThread;
        private List<NodeReference> _listIpPort = new List<NodeReference>(); 

        public ServerConnection() : this(DefaultPort)
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

        public ServerConnection(uint defaultPort, IPAddress ipAddress) : this(defaultPort, ipAddress, new LocalTime())
        {
        }

        public event EventHandler<Socket> OnStartListner;
        public event EventHandler<Socket> OnConnect;
        public event EventHandler<StateObject> OnReceive;
        public event EventHandler<int> OnSend;

        private void ListnerEvents()
        {
            _asynchronousSocketListener.OnStartListen += OnStartListenEvent;
            _asynchronousSocketListener.OnConnect += OnConnectEvent;
            _asynchronousSocketListener.OnReceive += OnReceiveEvent;
            _asynchronousSocketListener.OnSend += OnSendEvent;
        }

        private void OnStartListenEvent(object sender, Socket socket)
        {
            if (OnStartListner != null)
                new Thread(() => OnStartListner(this, socket)).Start();
        }

        private void OnSendEvent(object sender, int BytesReceived)
        {
            if (OnSend != null)
                new Thread(() => OnSend(sender, BytesReceived)).Start();
        }

        private void OnReceiveEvent(object sender, StateObject e)
        {
            if (!TryReplyKnownProtocol(e) && OnReceive != null)
                new Thread(() => OnReceive(sender, e)).Start();
        }

        protected override void HandleCorrectResponse(StateObject so, TimeSyncMessage message)
        {
            if (message is TimeSyncConnectRequest)
                Send(so.workSocket, buildTimeSyncConnectResponse(message as TimeSyncConnectRequest, so));
            else if (message is TimeSyncRequest)
                Send(so.workSocket, buildTimeSyncResponse(message as TimeSyncRequest, so.receiveTime));
            else if (message is TimeSyncConnectedClientsRequest)
                Send(so.workSocket, buildTimeSyncConnectedClientsResponse());
        }

        private string buildTimeSyncConnectResponse(TimeSyncConnectRequest message, StateObject so)
        {
            updateIPPortList(((IPEndPoint) so.workSocket.RemoteEndPoint).Address, message.NewConnectionPort);
            var response = new TimeSyncConnectResponse();
            response.ReturnCode = 0;
            return response.ToJSON();
        }

        private void updateIPPortList(IPAddress address, uint port)
        {
            var nodeReference = new NodeReference()
            {
                IpAddress = ConvertIPAddressToStringIp(address),
                Port = port
            };
            if (_listIpPort.Contains(nodeReference))
                return;
            _listIpPort.Add(nodeReference);
        }

        private static string ConvertIPAddressToStringIp(IPAddress address)
        {
            return string.Join(".", address.GetAddressBytes().Select(a => a.ToString("d")));
        }

        private string buildTimeSyncConnectedClientsResponse()
        {
            var IpsList =
                GetConnectedIpAddresses()
                    .Select(ConvertIPAddressToStringIp);
            var Message = new TimeSyncConnectedClientsResponse();
            var bufferList = new List<NodeReference>(_listIpPort);
            foreach (var ip in IpsList)
            {
                if ( _listIpPort.Exists( node => node.IpAddress == ip) )
                    continue;
                bufferList.Add( new NodeReference()
                    {
                        IpAddress =  ip,
                        Port = DefaultPort
                    }
                );
            }
            Message.ClientsIps = bufferList.ToArray();
            return Message.ToJSON();
        }

        public List<IPAddress> GetConnectedIpAddresses()
        {
            var IpsList = new List<IPAddress>();
            foreach (var socket in _socketList)
            {
                var ipEndPoint = socket.RemoteEndPoint as IPEndPoint;
                if (ipEndPoint != null)
                    IpsList.Add(ipEndPoint.Address);
            }
            return IpsList;
        }

        private string buildTimeSyncResponse(TimeSyncRequest message, DateTime receiveTime)
        {
            var response = new TimeSyncResponse();
            response.RequestTime = message.RequestTime;
            response.ReceivedTime = receiveTime.Add(_localTime.GetTimeSpan());
            response.ResponseTime = _localTime.GetDateTime();
            return response.ToJSON();
        }

        private void OnConnectEvent(object sender, Socket socket)
        {
            RegisterSocket(socket);
            if (OnConnect != null)
                new Thread(() => OnConnect(this, socket)).Start();
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
            catch (SocketException)
            {
//                throw new Exception("Port Alread Is Open");
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

        public IPAddress GetIP()
        {
            return _asynchronousSocketListener.GetIP();
        }

        public uint GetPort()
        {
            return _asynchronousSocketListener.GetPort();
        }

        public DateTime GetDateTime(bool localTime = true)
        {
            var dateTime = ((LocalTime) _localTime.Clone());
            return localTime ? dateTime.GetLocalDateTime() : dateTime.GetDateTime();
        }

        public List<NodeReference> GetConnectedNodes()
        {
            return new List<NodeReference>(_listIpPort);
        }
    }
}