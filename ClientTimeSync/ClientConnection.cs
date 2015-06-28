﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using ServerTimeSync;
using TimeSyncBase;
using TimeSyncBase.Connection;
using TimeSyncBase.messages;
using TimeSyncBase.messages.requests;
using TimeSyncBase.messages.responses;

namespace ClientTimeSync
{
	public class ClientConnection : ConnectionBase
	{
        public EventHandler<Socket> OnConnect;
        public EventHandler<StateObject> OnReceive;
        public EventHandler<int> OnSend;
        public EventHandler<DateTime> OnTimeSync;
        public EventHandler<List<IPAddress>> OnUpdateClientList;
        private AsynchronousClient _asynchronousClient;
	    private Thread _ClientThread;
	    private LocalTime _localTime;

	    public ClientConnection(string hostName, uint port = ServerConnection.DefaultPort):this(hostName,port,new LocalTime())
		{            
		}
	    public ClientConnection(string hostName, uint port, LocalTime localTimeClient)
	    {
	        _asynchronousClient = new AsynchronousClient(hostName, (int) port);
            ListnerEvents();
	        _localTime = localTimeClient;
	    }

	    public ClientConnection(string hostName, LocalTime localTimeClient):this(hostName: hostName, localTimeClient: localTimeClient, port:ServerConnection.DefaultPort)
	    {
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
                new Thread(() => OnSend(sender, BytesReceived)).Start();
        }

        private void OnReceiveEvent(object sender, StateObject e)
        {
            if (!TryReplyKnownProtocol(e) && OnReceive != null)
                new Thread(() => OnReceive(sender, e)).Start();
        }

        private void OnConnectEvent(object sender, Socket e)
        {
            if (OnConnect != null)
                new Thread(() => OnConnect(sender, e)).Start();
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

	    public void SyncTime()
	    {
	        TimeSyncRequest timeSyncRequest = new TimeSyncRequest();
	        timeSyncRequest.RequestTime = _localTime.GetDateTime();
            _asynchronousClient.Send(timeSyncRequest.ToJSON());
	    }

	    public LocalTime GetLocalTime()
	    {
	        return (LocalTime)_localTime.Clone();
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
	        foreach (string clientsIp in timeSyncConnectedClientsResponse.ClientsIps)
	        {
	            IPAddress bufferIp; 
	            if (IPAddress.TryParse(clientsIp,out bufferIp))
                    ipList.Add(bufferIp);
	        }

	        if (OnUpdateClientList != null)
	            new Thread(()=>OnUpdateClientList(this, ipList)).Start();
	    }

	    private void UpdateLocalTime(TimeSyncResponse timeSyncResponse, DateTime receiveTime)
	    {
	        DateTime remoteHour = Calculator.PullTimeSyncCalc(timeSyncResponse.RequestTime, timeSyncResponse.ReceivedTime,
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

