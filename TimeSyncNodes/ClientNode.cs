using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;
using ClientTimeSync;
using ServerTimeSync;
using Timer = System.Timers.Timer;

namespace TimeSync
{
    internal class ClientNode : ServerNode
    {
        private const string RemoteServerHash = "Server";
        private readonly Dictionary<string, ClientConnection> _clients = new Dictionary<string, ClientConnection>();
        private readonly bool _isRunning;
        private Timer _pullGetClients;
        private Timer _pullTimer;

        public ClientNode(string hostName)
        {
            InitializeClient(hostName);
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

        private void InitializeClient(string hostName, bool checkServerPort = false)
        {
            DefaultTimeOut = 60*1000;
            if (checkServerPort)
                AddNewItensInList(GetIpAddressFromHostName(hostName), RemoteServerHash);
            else
                AddNewItensInList(GetIpAddressFromHostName(hostName), RemoteServerHash);
            TryRegisterRemoteServerHostEvents();
            _pullTimer = new Timer(30*1000);
            _pullTimer.Elapsed += SendSyncMessage;
            _pullGetClients = new Timer(60*1000);
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

        private void UpdateClientList(object sender, List<IPAddress> ipAddresses)
        {
            foreach (var address in ipAddresses)
            {
                AddNewItensInList(address);
            }
        }

        public void AddNewItensInList(IPAddress ipAddress, string keyName = null)
        {
            if (!IsLocalIpAddress(ipAddress.ToString()) &&
                _clients.All(client => !Equals(client.Value.GetRemoteIpAddress(), ipAddress)))
            {
                var key = keyName ?? ipAddress.GetAddressBytes().ToString();
                var clientConnection = new ClientConnection(ipAddress.ToString());
                _clients.Add(key, clientConnection);
                _clients[key].OnDisconnect += OnDisconnectRemoveFromList;
                TryConnectNewAddress(key);
            }
        }

        private void TryConnectNewAddress(string keyName)
        {
            if (IsRunning)
            {
                if (_clients.ContainsKey(keyName))
                {
                    _clients[keyName].ConnectThreaded();
                }
            }
        }

        private void OnDisconnectRemoveFromList(object sender, Socket e)
        {
            if (_clients.ContainsKey(_clients.First(item => item.Value.Equals(sender)).Key))
                _clients.Remove(_clients.First(item => item.Value.Equals(sender)).Key);
        }

        private void SendGetClientsMessage(object sender, ElapsedEventArgs e)
        {
            _clients[RemoteServerHash].FoundNewClients();
        }

        private void SendSyncMessage(object sender, ElapsedEventArgs e)
        {
            foreach (var client in _clients)
            {
                client.Value.SyncTime();
            }
        }

        public override bool StartService()
        {
            base.StartService();
            new Thread(StartClients).Start();
            return true;
        }

        private void StartClients()
        {
            if (!ServerIsRunning.WaitOne(DefaultTimeOut)) return;
            foreach (var client in _clients)
            {
                client.Value.ConnectThreaded();
            }
        }

        public override void StopService()
        {
            base.StopService();
            StopClients();
        }

        public override List<IPAddress> GetActiveConnections()
        {
            return _clients.Values.Select(con => con.GetRemoteIpAddress()).ToList();
        }

        private void StopClients()
        {
            foreach (var client in _clients)
            {
                client.Value.Stop();
            }
        }

        protected bool IsLocalIpAddress(string host)
        {
            return false;
            try
            {
                // get host IP addresses
                var hostIPs = Dns.GetHostAddresses(host);
                // get local IP addresses
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (var hostIP in hostIPs)
                {
                    // is localhost
                    if (IPAddress.IsLoopback(hostIP)) return true;
                    // is local address
                    foreach (var localIP in localIPs)
                    {
                        if (hostIP.Equals(localIP)) return true;
                    }
                }
            }
            catch
            {
            }
            return false;
        }
    }
}