using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerTimeSync;
using TimeSyncBase;
using TimeSyncBase.Connection;
using TimeSyncBase.messages.responseless;

namespace TimeSyncNodes
{
    public class ServerNode : INode
    {
        private readonly ServerConnection _server;
        protected ManualResetEvent ServerIsRunning = new ManualResetEvent(false);
        protected LocalTime _serverLocalTime = new LocalTime();

        public ServerNode()
        {
            _server = new ServerConnection(ServerConnection.DefaultPort, IPAddress.Any, _serverLocalTime);
            IpAddress = _server.GetIP();
            Port = _server.GetPort();
            InitializeComponets();
        }

        public ServerNode(uint port)
        {
            Port = port;
            _server = new ServerConnection(Port,IPAddress.Any, _serverLocalTime);
            IpAddress = _server.GetIP();
            InitializeComponets();
        }

        public ServerNode(ServerConnection server)
        {
            _server = server;
            IpAddress = _server.GetIP();
            Port = _server.GetPort();
            InitializeComponets();
        }

        public ServerNode(uint port, IPAddress ipAddress)
        {
            Port = port;
            IpAddress = ipAddress;
            _server = new ServerConnection(Port, IpAddress, _serverLocalTime);
            InitializeComponets();
        }

        public bool IsRunning { get; private set; }
        public IPAddress IpAddress { get; set; }
        public uint Port { get; set; }
		public event Action<object,StateObject,TimeSyncResponseless> OnTimeSyncResponseLess;

        public virtual bool StartService()
        {
            _server.StartThreaded();
            return true;
        }

        public virtual void StopService()
        {
            _server.Stop();
            IsRunning = false;
        }

        public virtual List<IPAddress> GetActiveIPs()
        {
            return _server.GetConnectedIpAddresses();
        }

        public virtual List<ConnectionBase> GetActiveConnections()
        {
            var connectionBasesList = new List<ConnectionBase>();
            connectionBasesList.Add(_server);
            return connectionBasesList;
        }

        public List<NodeReference> GetActiveConnectionsNodes()
        {
            return _server.GetConnectedNodes();
        }

        public DateTime GetDateTime(bool localtime = true)
        {
            return _server.GetDateTime(localtime);
        }

        private void InitializeComponets()
        {
            _server.OnStartListner += OnStartListnerEvent;
			_server.OnTimeSyncResponseLess += OnTimeSyncResponseLessEvent; 
        }

        private void OnStartListnerEvent(object sender, Socket e)
        {
            IsRunning = true;
            ServerIsRunning.Set();
        }

		protected virtual void OnTimeSyncResponseLessEvent (object sender, StateObject so, TimeSyncResponseless message)
		{
			if (OnTimeSyncResponseLess != null)
				new Thread(() => OnTimeSyncResponseLess(sender, so,  message)).Start();			
		}

        public void UpdateDateTimeServer(DateTime newDateTime)
        {
            _serverLocalTime.SetDateTime(newDateTime);
        }
    }
}