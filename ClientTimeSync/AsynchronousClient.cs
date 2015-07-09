using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Text;
using ServerTimeSync;
using TimeSyncBase.Connection;

namespace ClientTimeSync
{
	public class AsynchronousClient : IDisposable{
		// The port number for the remote device.
		public int Port { get; protected set; }
		public IPAddress remoteIP { get; protected set;}
        public event EventHandler<Socket> OnConnect;
        public event EventHandler<Socket> OnDisconnect;
        public event EventHandler<StateObject> OnReceive;
        public event EventHandler<int> OnSend;

		// ManualResetEvent instances signal completion.
		private ManualResetEvent connectDone = 
			new ManualResetEvent(false);
		private ManualResetEvent CanExit =
            new ManualResetEvent(false);
        
		// The response from the remote device.
		private String response = String.Empty;
	    private Socket _client;

	    public AsynchronousClient (string hostName, int port)
		{
			remoteIP = IPAddress.Parse(hostName);
			Port = port;
		}

		private IPEndPoint getRemoteEndPoint()
		{
			// Establish the remote endpoint for the socket.
			// The name of the 
			// remote device is "host.contoso.com".
//			IPHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
            IPAddress ipAddress = remoteIP;
			return new IPEndPoint(ipAddress, Port);

		}
		public void ConnectClient() {
			// Connect to a remote device.
			try {
				
				IPEndPoint remoteEP = getRemoteEndPoint();

				// Create a TCP/IP socket.
				_client = new Socket(AddressFamily.InterNetwork,
				    SocketType.Stream, ProtocolType.Tcp);

				// Connect to the remote endpoint.
				_client.BeginConnect( remoteEP, 
					new AsyncCallback(ConnectCallback), _client);
				
                connectDone.WaitOne();

                _client.BeginDisconnect(true, new AsyncCallback(DisconnectCallback), _client);
               
				Receive();
			    CanExit.WaitOne();

				// Write the response to the console.
				Console.WriteLine("Response received : {0}", response);

                // Release the socket.
//				_client.Shutdown(SocketShutdown.Both);
//				_client.Close();

			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

	    private void DisconnectCallback(IAsyncResult ar)
	    {
	        try
	        {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

	            if (OnDisconnect != null)
	                OnDisconnect(this, client);
	        }
	        catch (Exception e)
	        {
	            Console.WriteLine(e.ToString());
	        }
	    }

	    private void ConnectCallback(IAsyncResult ar) {
            if (CanExit.WaitOne(0)) return;
			try {
				// Retrieve the socket from the state object.
				Socket client = (Socket) ar.AsyncState;

				// Complete the connection.
				client.EndConnect(ar);

				Console.WriteLine("Socket connected to {0}",
					client.RemoteEndPoint.ToString());

				// Signal that the connection has been made.
				connectDone.Set();
			    if (OnConnect != null)
			        OnConnect(this, client);
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private void Receive() {
            if (CanExit.WaitOne(0)) return;
			try {
				// Create the state object.
				StateObject state = new StateObject();
				state.workSocket = _client;

				// Begin receiving the data from the remote device.
				_client.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
					new AsyncCallback(ReceiveCallback), state);
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		private void ReceiveCallback( IAsyncResult ar ) {
            if (CanExit.WaitOne(0)) return;
			try {
                DateTime receiveTime = DateTime.Now;
				// Retrieve the state object and the client socket 
				// from the asynchronous state object.
				StateObject state = (StateObject) ar.AsyncState;
				Socket client = state.workSocket;

				// Read data from the remote device.
				int bytesRead = client.EndReceive(ar);

				if (bytesRead > 0) {
					// There might be more data, so store the data received so far.
					state.sb.Append(Encoding.ASCII.GetString(state.buffer,0,bytesRead));
                    state.receiveTime = receiveTime;
					// Get the rest of the data.
                    if (OnReceive != null)
                        OnReceive(this, (StateObject)state.Clone());
				} 

                if (!CanExit.WaitOne(0, false))
                {
                    Receive();
                }
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

		public void Send(String data) {
            if (CanExit.WaitOne(0)) return;
		    // Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			_client.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), _client);
		}

		private void SendCallback(IAsyncResult ar) {
            if (CanExit.WaitOne(0)) return;
			try {
				// Retrieve the socket from the state object.
				Socket client = (Socket) ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = client.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to server.", bytesSent);

				// Signal that all bytes have been sent.
                if (OnSend != null)
                    OnSend(this, bytesSent);
			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

	    public void Dispose()
	    {
            _client.Shutdown(SocketShutdown.Both);
            _client.Dispose();
	        CanExit.Set();
			while (_client.Connected) ;
	    }
	}
}