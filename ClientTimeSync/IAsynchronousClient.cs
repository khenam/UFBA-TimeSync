using System;
using System.Net;
using System.Net.Sockets;
using TimeSyncBase.Connection;

namespace ClientTimeSync
{
    public interface IAsynchronousClient : IDisposable
    {
        event EventHandler<Socket> OnConnect;
        event EventHandler<StateObject> OnReceive;
        event EventHandler<int> OnSend;
        event EventHandler<Socket> OnDisconnect;
        IPAddress remoteIP { get;}
        int RemotePort { get;}
        void ConnectClient();
        void Send(string message);
    }
}