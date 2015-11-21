using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using ClientTimeSync;
using ServerTimeSync;
using TimeSyncBase;
using TimeSyncBase.Connection;
using Timer = System.Timers.Timer;

namespace TimeSyncNodes
{
    public class ClientNode : ServerNode
    {
        private const string RemoteServerHash = "Server";
        private readonly Dictionary<string, ClientConnection> _clients = new Dictionary<string, ClientConnection>();
        private bool _clientIsRunning = false;
        private Timer _pullGetClients;
        private Timer _pullTimer;
		private Timer _checkClientsConnections;
        private uint _lowerPullSyncInterval = 100;
        private uint _lowerPullSyncClientsInterval = 1000;
        public EventHandler<List<ConnectionBase>> OnNodesConnectedChange;
		private Dictionary<ConnectionBase,DateTime> lastReceiveMessage = new Dictionary<ConnectionBase, DateTime>();

        public ClientNode(string hostName)
        {
            InitializeClient(hostName);
        }

        public ClientNode(string hostName, uint port)
        {
            InitializeClient(hostName, true, port);
        }

        public ClientNode(ServerConnection server, string hostName)
            : base(server)
        {
            InitializeClient(hostName);
        }

        public ClientNode(uint serverPort, IPAddress serverIpAddress, string hostName)
            : base(serverPort, serverIpAddress)
        {
            InitializeClient(hostName, true);
        }

        public int DefaultTimeOut { get; private set; }

        private void InitializeClient(string hostName, bool checkServerPort = false, uint? port = null)
        {
            DefaultTimeOut = 10*1000;
            if (checkServerPort)
                AddNewItensInList(GetIpAddressFromHostName(hostName), port, _serverLocalTime, RemoteServerHash);
            else
                AddNewItensInList(GetIpAddressFromHostName(hostName), null, _serverLocalTime, RemoteServerHash);
            TryRegisterRemoteServerHostEvents();
			_pullTimer = new Timer(DefaultTimeOut);
            _pullTimer.Elapsed += SendSyncMessage;
			_checkClientsConnections = new Timer(DefaultTimeOut * 5);
			_checkClientsConnections.Elapsed += CheckClients; 
			_pullGetClients = new Timer(DefaultTimeOut * 1.5);
            _pullGetClients.Elapsed += SendGetClientsMessage;
        }

        private void TryRegisterRemoteServerHostEvents()
        {
            if (_clients.ContainsKey(RemoteServerHash))
                _clients[RemoteServerHash].OnUpdateClientList += UpdateClientList;
        }

        private static IPAddress GetIpAddressFromHostName(string hostName)
        {
            IPAddress ip;
            if (!IPAddress.TryParse(hostName, out ip))
            {
                var ipHostInfo = Dns.GetHostEntry(hostName);
                ip = ipHostInfo.AddressList[0];
            }
            return ip;
        }

        private void UpdateClientList(object sender, List<NodeReference> ipAddresses)
        {
            foreach (var address in ipAddresses)
            {
                IPAddress bufferIp;
				if (IPAddress.TryParse (address.IpAddress, out bufferIp))
					AddNewItensInList(bufferIp, Port);
            }
        }

        public void AddNewItensInList(IPAddress ipAddress, uint? port = null, LocalTime localTime = null, string keyName = null)
        {
            if (!IsLocalIpAddress(ipAddress.ToString(), port) &&
                _clients.All(client => !(Equals(client.Value.GetRemoteIpAddress(), ipAddress) && Equals(client.Value.GetRemotePort(), port.HasValue ? port.Value : client.Value.GetRemotePort()))))
            {
                var key = keyName ?? ipAddress.ToString();
                ClientConnection clientConnection;
                if (localTime == null)
                    clientConnection = port.HasValue
                        ? new ClientConnection(ipAddress.ToString(), port.Value)
                        : new ClientConnection(ipAddress.ToString());
                else
                    clientConnection = port.HasValue
                        ? new ClientConnection(ipAddress.ToString(), port.Value, localTime)
                        : new ClientConnection(ipAddress.ToString(),ClientConnection.DefaultPort,localTime);

                _clients.Add(key, clientConnection);
                _clients[key].OnDisconnect += OnDisconnectRemoveFromList;
				_clients[key].OnTimeSync += OnTimeSyncEvent;
                TryConnectNewAddress(key);
				UpdateLastTimeConnection (clientConnection);

                if (OnNodesConnectedChange != null)
                    new Thread(() => OnNodesConnectedChange(this, GetActiveConnections())).Start();
            }
        }

        private void TryConnectNewAddress(string keyName)
        {
            if (_clientIsRunning)
            {
                if (_clients.ContainsKey(keyName))
                {
                    if (!_clients[keyName].IsConnected)
                        _clients[keyName].ConnectThreaded();
                }
            }
        }

        private void OnDisconnectRemoveFromList(object sender, Socket e)
        {
            if (_clients.ContainsKey(_clients.First(item => item.Value.Equals(sender)).Key))
                _clients.Remove(_clients.First(item => item.Value.Equals(sender)).Key);
            if (OnNodesConnectedChange != null)
                new Thread(() => OnNodesConnectedChange(this, GetActiveConnections())).Start();
        }

		void OnTimeSyncEvent (object sender, DateTime e)
		{
			if (_clients.ContainsKey(_clients.First(item => item.Value.Equals((ConnectionBase)sender)).Key))
			{
				var connection = _clients.First (item => item.Value.Equals ((ConnectionBase)sender)).Value;
				UpdateLastTimeConnection (connection);
			}
		}

		void UpdateLastTimeConnection (ConnectionBase connection)
		{
			if (lastReceiveMessage.ContainsKey (connection))
				lastReceiveMessage [connection] = DateTime.UtcNow;
			else
				lastReceiveMessage.Add (connection, DateTime.UtcNow);
		}

        private void SendGetClientsMessage(object sender, ElapsedEventArgs e)
        {
            _clients[RemoteServerHash].FoundNewClients();
        }

		private void CheckClients(object sender, ElapsedEventArgs e)
		{
			uint referenceSeconds = GetPullSyncTime () / 1000 * 10;
			foreach (var last in lastReceiveMessage) 
			{
				if ( _clients.Any(item => item.Value.Equals (last.Key)) && IsTimeExceeded (last.Value,referenceSeconds)) 
				{
					_clients.Remove (_clients.First (item => item.Value.Equals (last.Key)).Key);
					//lastReceiveMessage.Remove (last.Key);
				}
			}
		}

		bool IsTimeExceeded (DateTime dateTimeConnection, uint? referenceSeconds = null)
		{
			var testedReferenceSeconds = (!referenceSeconds.HasValue)? GetPullSyncTime () / 1000 * 2.5 : referenceSeconds.Value;
			return DateTime.UtcNow.Subtract(dateTimeConnection.ToUniversalTime()).TotalSeconds > (testedReferenceSeconds);
		}

        private void SendSyncMessage(object sender, ElapsedEventArgs e)
        {
            foreach (var client in _clients)
            {
                client.Value.SyncTime();
            }
            if (OnNodesConnectedChange != null)
                new Thread(() => OnNodesConnectedChange(this, GetActiveConnections())).Start();
        }

        public override bool StartService()
        {
            base.StartService();
            new Thread(StartClients).Start();
            _pullTimer.Enabled = true;
            _pullGetClients.Enabled = true;
			//_checkClientsConnections.Enabled = true;
            return _clientIsRunning = true;
        }

        private void StartClients()
        {
//            TryConnectNewAddress(RemoteServerHash);
//            if (!ServerIsRunning.WaitOne(DefaultTimeOut)) return;
            foreach (var client in _clients)
            {
                client.Value.ConnectThreaded();
            }
        }

        public override void StopService()
        {
            _pullTimer.Enabled = false;
            _pullGetClients.Enabled = false;
			_checkClientsConnections.Enabled = false;
            base.StopService();
            StopClients();
            _clientIsRunning = false;
        }

        public override List<IPAddress> GetActiveIPs()
        {
            return _clients.Values.Select(con => con.GetRemoteIpAddress()).ToList();
        }

        public new List<NodeReference> GetActiveConnectionsNodes()
        {
            return _clients.Values.Select(con => new NodeReference() { IpAddress = con.GetRemoteIpAddress().ToString(), Port = con.GetRemotePort() }).ToList();
        }


        private void StopClients()
        {
            foreach (var client in _clients)
            {
                client.Value.Stop();
            }
        }

        protected bool IsLocalIpAddress(string host, uint? port = null)
        {
//            return false;
            try
            {
                // get host IP addresses
                var hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (var hostIp in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIp) && Port == (port.HasValue? port.Value : Port)) return true;
                    // is local address
                    foreach (var localIp in localIPs)
                    {
                        if (hostIp.Equals(localIp) && Port == (port.HasValue ? port.Value : Port)) return true;
                    }
                }
            }
            catch
            {
                // ignored
            }
            return false;
        }

        public override List<ConnectionBase> GetActiveConnections()
        {
			return _clients.Values.Cast<ConnectionBase>().Where(item => {
				if (lastReceiveMessage.ContainsKey (item))
					return !IsTimeExceeded(lastReceiveMessage[item]);
				else
					return true;
			}).ToList();
        }

        public void SetPullSyncTime(uint interval)
        {
            _pullTimer.Interval = interval > _lowerPullSyncInterval ? interval : _lowerPullSyncInterval;
			_checkClientsConnections.Interval = _pullTimer.Interval * 2;
        }

        public uint GetPullSyncTime()
        {
            return (uint) _pullTimer.Interval;
        }

        public void SetPullSyncClients(uint interval)
        {
            _pullGetClients.Interval = interval > _lowerPullSyncClientsInterval ? interval : _lowerPullSyncClientsInterval;
        }

        public uint GetPullSyncClients()
        {
            return (uint) _pullGetClients.Interval;
        }
    }
}