using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TimeSyncBase.Connection;

namespace ClientTimeSync
{
    public class UdpAsynchronousClient : IAsynchronousClient
    {
        private readonly ManualResetEvent CanExit =
            new ManualResetEvent(false);

        // ManualResetEvent instances signal completion.
        private readonly ManualResetEvent connectDone =
            new ManualResetEvent(false);

        // The response from the remote device.
        private readonly string response = string.Empty;
        private Socket _client;

        public UdpAsynchronousClient(string hostName, int remotePort)
        {
            remoteIP = IPAddress.Parse(hostName);
            RemotePort = remotePort;
        }

        // The port number for the remote device.
        public int RemotePort { get; protected set; }
        public IPAddress remoteIP { get; protected set; }

        public void Dispose()
        {
            lock (this)
            {
                _client.Shutdown(SocketShutdown.Both);
//                _client.BeginDisconnect(true, DisconnectCallback, _client);
                _client.Dispose();
                CanExit.Set();
            }
            while (_client.Connected) ;
        }

        public event EventHandler<Socket> OnConnect;
        public event EventHandler<Socket> OnDisconnect;
        public event EventHandler<StateObject> OnReceive;
        public event EventHandler<int> OnSend;

        private IPEndPoint getRemoteEndPoint()
        {
            // Establish the remote endpoint for the socket.
            // The name of the 
            // remote device is "host.contoso.com".
//			IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
            var ipAddress = remoteIP;
            return new IPEndPoint(ipAddress, RemotePort);
        }

        public void ConnectClient()
        {
            // Connect to a remote device.
            try
            {
                //var remoteEP = getRemoteEndPoint();

                // Create a UDP/IP socket.
                _client = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

                // Connect to the remote endpoint.
                //_client.BeginConnect(remoteEP, ConnectCallback, _client);

                //connectDone.WaitOne();
				if (OnConnect != null)
					OnConnect(this, _client);
				
                Receive();
                CanExit.WaitOne();
//                lock (this)
//                {
//                    _client.BeginDisconnect(true, DisconnectCallback, _client);
//                }

                // Write the response to the console.
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.
//				_client.Shutdown(SocketShutdown.Both);
//				_client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }

        private void DisconnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket) ar.AsyncState;

                if (OnDisconnect != null)
                    OnDisconnect(this, client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            if (CanExit.WaitOne(0)) return;
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket) ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                // Signal that the connection has been made.
                connectDone.Set();
//                if (OnConnect != null)
//                    OnConnect(this, client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void Receive()
        {
            if (CanExit.WaitOne(0)) return;
            try
            {
                lock (this)
                {
                    // Create the state object.
                    var state = new StateObject();
                    state.workSocket = _client;
                    state.RemoteEndPoint = getRemoteEndPoint();
                    // Begin receiving the data from the remote device.
                    _client.BeginReceiveFrom(state.buffer, 0, StateObject.BufferSize, 0, ref state.RemoteEndPoint,
						new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            if (CanExit.WaitOne(0)) return;
            try
            {
                DateTime receiveTime;
                StateObject state;
                int bytesRead;
                lock (this) 
                {
                    if (CanExit.WaitOne(0)) return;
           
                    receiveTime = DateTime.Now;
                    // Retrieve the state object and the client socket 
                    // from the asynchronous state object.
                    state = (StateObject) ar.AsyncState;
                    var client = state.workSocket;

                    // Read data from the remote device.
                    bytesRead = client.EndReceiveFrom(ar,ref state.RemoteEndPoint);
                }

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    state.receiveTime = receiveTime;
                    // Get the rest of the data.
                    if (OnReceive != null)
                        OnReceive(this, (StateObject) state.Clone());
                }

                if (!CanExit.WaitOne(0, false))
                {
                    Receive();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Send(string data)
        {
            if (CanExit.WaitOne(0)) return;
            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(data);
            lock (this)
            {
                if (CanExit.WaitOne(0)) return;
                // Begin sending the data to the remote device.
                
				_client.BeginSendTo(byteData, 0, byteData.Length, SocketFlags.None,getRemoteEndPoint(),
					new AsyncCallback(SendCallback), _client);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            if (CanExit.WaitOne(0)) return;
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
				var bytesSent = client.EndSendTo(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                if (OnSend != null)
                    OnSend(this, bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}