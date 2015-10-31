﻿using System;
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
        private Thread _clientThread;
        public EventHandler<Socket> OnConnect;
        public EventHandler<Socket> OnDisconnect;
        public EventHandler<StateObject> OnReceive;
        public EventHandler<int> OnSend;
        public EventHandler<DateTime> OnTimeSync;
        public EventHandler<List<NodeReference>> OnUpdateClientList;

        public bool IsConnected { get; private set; }

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
        public uint GetRemotePort()
        {
            return (uint) _asynchronousClient.RemotePort;
        }

        private void OnSendEvent(object sender, int bytesReceived)
        {
            if (OnSend != null)
                new Thread(() => OnSend(this, bytesReceived)).Start();
        }

        private void OnReceiveEvent(object sender, StateObject e)
        {
            if (!TryReplyKnownProtocol(e) && OnReceive != null)
                new Thread(() => OnReceive(this, e)).Start();
        }

        private void OnConnectEvent(object sender, Socket e)
        {
            IsConnected = true;
            if (OnConnect != null)
                new Thread(() => OnConnect(this, e)).Start();

        }

        private void OnDisconnectEvent(object sender, Socket e)
        {
            IsConnected = false;
            if (OnDisconnect != null)
                new Thread(() => OnDisconnect(this, e)).Start();
        }

        public void Connect()
        {
            ConnectFunctionThread();
        }

        public void ConnectThreaded()
        {
            _clientThread = new Thread(ConnectFunctionThread);
            _clientThread.Start();
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
            if ((_clientThread != null) && _clientThread.IsAlive)
            {
//	            _ClientThread.Abort();
                _clientThread.Join();
            }
        }

        public void SyncTime()
        {
            var timeSyncRequest = new TimeSyncRequest();
            timeSyncRequest.RequestTime = _localTime.GetDateTime();
            _asynchronousClient.Send(timeSyncRequest.ToJSON());
        }

        public override IPAddress GetIP()
        {
            return GetRemoteIpAddress();
        }

        public override uint GetPort()
        {
            return GetRemotePort();
        }

        public override LocalTime GetLocalTime()
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

        private void UpdateClientsConnected(TimeSyncConnectedClientsResponse message)
        {
            var ipList = new List<NodeReference>();
            foreach (var clientsIp in message.ClientsIps)
            {
//                IPAddress bufferIp;
//                if (IPAddress.TryParse(clientsIp.IpAddress, out bufferIp))
                    ipList.Add(new NodeReference()
                    {
                        IpAddress = clientsIp.IpAddress,
                        Port = clientsIp.Port
                    });
            }

            if (OnUpdateClientList != null)
                new Thread(() => OnUpdateClientList(this, ipList)).Start();
        }

        private void UpdateLocalTime(TimeSyncResponse message, DateTime receiveTime)
        {
            var responseTimeConverted = receiveTime.Subtract(_localTime.GetTimeSpan());
            var remoteHour = Calculator.PullTimeSyncCalc(message.RequestTime, message.ReceivedTime,
                message.ResponseTime, responseTimeConverted);

            _localTime.SetTimeSpan(-responseTimeConverted.Subtract(remoteHour));

            if (OnTimeSync != null)
                new Thread(() => OnTimeSync(this, _localTime.GetDateTime())).Start();
        }

        public void FoundNewClients()
        {
            if (IsConnected)
                _asynchronousClient.Send((new TimeSyncConnectedClientsRequest()).ToJSON());
        }
    }
}