using System;
using System.Net.Sockets;
using System.Text;

namespace TimeSyncBase.Connection
{
    public class StateObject : ICloneable
    {
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        public DateTime receiveTime = DateTime.MinValue;
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        // Client  socket.
        public Socket workSocket;

        public object Clone()
        {
            return new StateObject
            {
                workSocket = workSocket,
                buffer = (byte[]) buffer.Clone(),
                sb = new StringBuilder(sb.ToString()),
                receiveTime = new DateTime(receiveTime.Ticks)
            };
        }
    }
}