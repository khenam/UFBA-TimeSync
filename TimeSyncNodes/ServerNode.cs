using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerTimeSync;

namespace TimeSync
{
    internal class ServerNode : INode
    {
        private readonly ServerConnection _server;
        protected ManualResetEvent ServerIsRunning = new ManualResetEvent(false);

        public ServerNode()
        {
            _server = new ServerConnection();
            IpAddress = _server.GetIP();
            Port = _server.GetPort();
        }

        public ServerNode(ServerConnection server)
        {
            _server = server;
            IpAddress = _server.GetIP();
            Port = _server.GetPort();
        }

        public ServerNode(uint port, IPAddress ipAddress)
        {
            Port = port;
            IpAddress = ipAddress;
            _server = new ServerConnection(Port, IpAddress);
            InitializeComponets();
        }

        public bool IsRunning { get; private set; }
        public IPAddress IpAddress { get; set; }
        public uint Port { get; set; }

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

        public virtual List<IPAddress> GetActiveConnections()
        {
            return _server.GetConnectedIpAddresses();
        }

        public DateTime GetDateTime(bool localtime = true)
        {
            return _server.GetDateTime(localtime);
        }

        private void InitializeComponets()
        {
            _server.OnStartListner += OnStartListnerEvent;
        }

        private void OnStartListnerEvent(object sender, Socket e)
        {
            IsRunning = true;
            ServerIsRunning.Set();
        }
    }
}