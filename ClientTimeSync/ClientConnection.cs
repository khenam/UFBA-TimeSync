using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using TimeSyncBase;
using TimeSyncBase.Connection;
using TimeSyncBase.messages;
using TimeSyncBase.messages.requests;
using TimeSyncBase.messages.responses;

namespace ClientTimeSync
{
    public class ClientConnection : ConnectionBase
    {
        private readonly AsynchronousClient _asynchronousClient;
        private readonly LocalTime _localTime;
        private Thread _ClientThread;
        public EventHandler<Socket> OnConnect;
        public EventHandler<Socket> OnDisconnect;
        public EventHandler<StateObject> OnReceive;
        public EventHandler<int> OnSend;
        public EventHandler<DateTime> OnTimeSync;
        public EventHandler<List<IPAddress>> OnUpdateClientList;

        public ClientConnection(string hostName, uint port = DefaultPort) : this(hostName, port, new LocalTime())
        {
        }

        public ClientConnection(string hostName, uint port, LocalTime localTimeClient)
        {
            _asynchronousClient = new AsynchronousClient(hostName, (int) port);
            ListnerEvents();
            _localTime = localTimeClient;
        }

        public ClientConnection(string hostName, LocalTime localTimeClient)
            : this(hostName, localTimeClient: localTimeClient, port: DefaultPort)
        {
        }

        private void ListnerEvents()
        {
            _asynchronousClient.OnConnect += OnConnectEvent;
            _asynchronousClient.OnReceive += OnReceiveEvent;
            _asynchronousClient.OnSend += OnSendEvent;
            _asynchronousClient.OnDisconnect += OnDisconnectEvent;
        }

        public IPAddress GetRemoteIpAddress()
        {
            return _asynchronousClient.remoteIP;
        }

        private void OnSendEvent(object sender, int BytesReceived)
        {
            if (OnSend != null)
                new Thread(() => OnSend(this, BytesReceived)).Start();
        }

        private void OnReceiveEvent(object sender, StateObject e)
        {
            if (!TryReplyKnownProtocol(e) && OnReceive != null)
                new Thread(() => OnReceive(this, e)).Start();
        }

        private void OnConnectEvent(object sender, Socket e)
        {
            if (OnConnect != null)
                new Thread(() => OnConnect(this, e)).Start();
        }

        private void OnDisconnectEvent(object sender, Socket e)
        {
            if (OnDisconnect != null)
                new Thread(() => OnDisconnect(this, e)).Start();
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
            catch (SocketException)
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

        public void SyncTime()
        {
            var timeSyncRequest = new TimeSyncRequest();
            timeSyncRequest.RequestTime = _localTime.GetDateTime();
            _asynchronousClient.Send(timeSyncRequest.ToJSON());
        }

        public LocalTime GetLocalTime()
        {
            return (LocalTime) _localTime.Clone();
        }

        protected override void HandleCorrectResponse(StateObject so, TimeSyncMessage message)
        {
            if (message is TimeSyncResponse)
                UpdateLocalTime(message as TimeSyncResponse, so.receiveTime);
            else if (message is TimeSyncConnectedClientsResponse)
                UpdateClientsConnected(message as TimeSyncConnectedClientsResponse);
        }

        private void UpdateClientsConnected(TimeSyncConnectedClientsResponse timeSyncConnectedClientsResponse)
        {
            var ipList = new List<IPAddress>();
            foreach (var clientsIp in timeSyncConnectedClientsResponse.ClientsIps)
            {
                IPAddress bufferIp;
                if (IPAddress.TryParse(clientsIp.IpAddress, out bufferIp))
                    ipList.Add(bufferIp);
            }

            if (OnUpdateClientList != null)
                new Thread(() => OnUpdateClientList(this, ipList)).Start();
        }

        private void UpdateLocalTime(TimeSyncResponse timeSyncResponse, DateTime receiveTime)
        {
            var remoteHour = Calculator.PullTimeSyncCalc(timeSyncResponse.RequestTime, timeSyncResponse.ReceivedTime,
                timeSyncResponse.ResponseTime, receiveTime);

            _localTime.SetTimeSpan(-receiveTime.Subtract(remoteHour));

            if (OnTimeSync != null)
                new Thread(() => OnTimeSync(this, _localTime.GetDateTime())).Start();
        }

        public void FoundNewClients()
        {
            _asynchronousClient.Send((new TimeSyncConnectedClientsRequest()).ToJSON());
        }
    }
}