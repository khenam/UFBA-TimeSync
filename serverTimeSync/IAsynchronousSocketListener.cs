using System;
using System.Net;
using System.Net.Sockets;
using TimeSyncBase.Connection;

namespace ServerTimeSync
{
    public interface IAsynchronousSocketListener : IDisposable
    {
        event EventHandler<Socket> OnStartListen;
        event EventHandler<Socket> OnConnect;
        event EventHandler<StateObject> OnReceive;
        event EventHandler<int> OnSend;
        IPAddress GetIP();
        uint GetPort();
        void StartListening();
		void Send(StateObject handler, string messsage);
    }
}