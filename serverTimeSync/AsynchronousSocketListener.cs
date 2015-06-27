using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ServerTimeSync
{
	// State object for reading client data asynchronously
	public class StateObject {
		// Client  socket.
		public Socket workSocket = null;
		// Size of receive buffer.
		public const int BufferSize = 1024;
		// Receive buffer.
		public byte[] buffer = new byte[BufferSize];
		// Received data string.
		public StringBuilder sb = new StringBuilder();  
	}

    public class AsynchronousSocketListener : IDisposable
    {
		// Thread signal.
		private ManualResetEvent allDone = new ManualResetEvent(false);
        private ManualResetEvent CanExit = new ManualResetEvent(false);
        public event EventHandler<Socket> OnConnect;
        public event EventHandler<StateObject> OnReceive;
        public event EventHandler<int> OnSend;

	    private IPAddress _ipAddress;
	    private uint _port;
		public AsynchronousSocketListener(uint defaultPort, IPAddress ipAddress=null)
		{
		    _ipAddress = ipAddress;
		    _port = defaultPort;
		}

	    public void StartListening(){
			// Data buffer for incoming data.
			byte[] bytes = new Byte[1024];

			// Establish the local endpoint for the socket.
			// The DNS name of the computer
			// running the listener is "host.contoso.com".
		    IPHostEntry ipHostInfo;
		    IPAddress ipAddress;
		    if (_ipAddress == null)
		    {
                ipHostInfo = Dns.Resolve(Dns.GetHostName());
                ipAddress = ipHostInfo.AddressList[0];    
		    }
		    else
		    {
		        ipAddress = _ipAddress;
		    }
			IPEndPoint localEndPoint = new IPEndPoint(ipAddress, (int) _port);

			// Create a TCP/IP socket.
			Socket listener = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp );

			// Bind the socket to the local endpoint and listen for incoming connections.
			try {
				listener.Bind(localEndPoint);
				listener.Listen(100);
                while (!CanExit.WaitOne(0,false)) {
					// Set the event to nonsignaled state.
					allDone.Reset();

					// Start an asynchronous socket to listen for connections.
					Console.WriteLine("Waiting for a connection...");
					listener.BeginAccept( 
						new AsyncCallback(AcceptCallback),
						listener );

					// Wait until a connection is made before continuing.
					allDone.WaitOne();
				}

			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}

            listener.Shutdown(SocketShutdown.Both);
            listener.Close();
			Console.WriteLine("\nPress ENTER to continue...");
			Console.Read();

		}

		public void AcceptCallback(IAsyncResult ar) {
			// Signal the main thread to continue.
			allDone.Set();
            
			// Get the socket that handles the client request.
			Socket listener = (Socket) ar.AsyncState;
			Socket handler = listener.EndAccept(ar);
            
            if (OnConnect != null)
                OnConnect(this, listener);

			// Create the state object.
			StateObject state = new StateObject();
			state.workSocket = handler;
			handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
				new AsyncCallback(ReadCallback), state);
		}

		public void ReadCallback(IAsyncResult ar) {
			String content = String.Empty;

			// Retrieve the state object and the handler socket
			// from the asynchronous state object.
			StateObject state = (StateObject) ar.AsyncState;
			Socket handler = state.workSocket;

			// Read data from the client socket. 
			int bytesRead = handler.EndReceive(ar);

			if (bytesRead > 0) {
				// There  might be more data, so store the data received so far.
				state.sb.Append(Encoding.ASCII.GetString(
					state.buffer,0,bytesRead));

				// Check for end-of-file tag. If it is not there, read 
				// more data.
//                content = state.sb.ToString();
			    
                if (OnReceive != null)
                    OnReceive(this, state);
                state.sb.Clear();
			    
			}

            if (!CanExit.WaitOne(0, false))
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
		}

		public void Send(Socket handler, String data) {
			// Convert the string data to byte data using ASCII encoding.
			byte[] byteData = Encoding.ASCII.GetBytes(data);

			// Begin sending the data to the remote device.
			handler.BeginSend(byteData, 0, byteData.Length, 0,
				new AsyncCallback(SendCallback), handler);
		}

		private void SendCallback(IAsyncResult ar) {
			try {
				// Retrieve the socket from the state object.
				Socket handler = (Socket) ar.AsyncState;

				// Complete sending the data to the remote device.
				int bytesSent = handler.EndSend(ar);
				Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                if (OnSend!= null)
                    OnSend(this, bytesSent);

//				handler.Shutdown(SocketShutdown.Both);
//				handler.Close();

			} catch (Exception e) {
				Console.WriteLine(e.ToString());
			}
		}

        public void Dispose()
        {
            CanExit.Set();
            allDone.Set();
        }
	}
}