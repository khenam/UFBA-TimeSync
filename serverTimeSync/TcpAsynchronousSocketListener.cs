﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TimeSyncBase.Connection;

namespace ServerTimeSync
{
    // State object for reading client data asynchronously
    public class TcpAsynchronousSocketListener : IAsynchronousSocketListener
    {
        private readonly IPAddress _ipAddress;
        private readonly uint _port;
        // Thread signal.
        private readonly ManualResetEvent allDone = new ManualResetEvent(false);
        private readonly ManualResetEvent CanExit = new ManualResetEvent(false);
        private Socket _listener;

        public TcpAsynchronousSocketListener(uint defaultPort, IPAddress ipAddress = null)
        {
            _ipAddress = ipAddress;
            _port = defaultPort;
        }

        public void Dispose()
        {
            _listener.Dispose();
            CanExit.Set();
            allDone.Set();
            while (_listener.Connected) ;
        }

        public event EventHandler<Socket> OnStartListen;
        public event EventHandler<Socket> OnConnect;
        public event EventHandler<StateObject> OnReceive;
        public event EventHandler<int> OnSend;

        public void StartListening()
        {
            // Data buffer for incoming data.

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPHostEntry ipHostInfo;
            IPAddress ipAddress;
            if (_ipAddress == null)
            {
                ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                ipAddress = ipHostInfo.AddressList[0];
            }
            else
            {
                ipAddress = _ipAddress;
            }
            var localEndPoint = new IPEndPoint(ipAddress, (int) _port);

            // Create a TCP/IP socket.
            _listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(100);
                CanExit.Reset();
                if (OnStartListen != null)
                    OnStartListen(this, _listener);
                while (!CanExit.WaitOne(0))
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    _listener.BeginAccept(
                        AcceptCallback,
                        _listener);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

//                _listener.Shutdown(SocketShutdown.Both);
//                _listener.Disconnect(false);
//                _listener.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            //Console.WriteLine("\nPress ENTER to continue...");
            //Console.Read();
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();
            if (CanExit.WaitOne(0)) return;

            // Get the socket that handles the client request.
            var listener = (Socket) ar.AsyncState;
            var handler = listener.EndAccept(ar);

            if (OnConnect != null)
                OnConnect(this, handler);

            // Create the state object.
            var state = new StateObject();
            state.workSocket = handler;
            state.RemoteEndPoint = state.workSocket.RemoteEndPoint;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            var receiveTime = DateTime.Now;
            if (CanExit.WaitOne(0)) return;
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject) ar.AsyncState;
            var handler = state.workSocket;

            // Read data from the client socket. 
            var bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (System.Exception)
            {
                // ignored
            }
            
            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));
                state.receiveTime = receiveTime;
                // Check for end-of-file tag. If it is not there, read 
                // more data.
//                content = state.sb.ToString();

                if (OnReceive != null)
                    OnReceive(this, (StateObject) state.Clone());
                state.sb.Clear();
            }

            if (!CanExit.WaitOne(0))
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    ReadCallback, state);
            }
        }

		public void Send(StateObject so, string data)
        {
            if (CanExit.WaitOne(0)) return;
            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
			so.workSocket.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, so);
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (CanExit.WaitOne(0)) return;
            try
            {
                // Retrieve the socket from the state object.
                var handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                var bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                if (OnSend != null)
                    OnSend(this, bytesSent);

//				handler.Shutdown(SocketShutdown.Both);
//				handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public IPAddress GetIP()
        {
            return _ipAddress;
        }

        public uint GetPort()
        {
            return _port;
        }
    }
}