using System;
using System.Net.Sockets;
using System.Text;

namespace TimeSyncBase.Connection
{
    public class StateObject : ICloneable
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
        public DateTime receiveTime = DateTime.MinValue;
        public object Clone()
        {
            return new StateObject()
            {
                workSocket = this.workSocket,
                buffer=(byte[])this.buffer.Clone(),
                sb=new StringBuilder(this.sb.ToString()),
                receiveTime = new DateTime(this.receiveTime.Ticks)
            };
        }
    }
}