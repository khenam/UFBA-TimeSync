using System;
using System.Net.Sockets;
using System.Text;
using TimeSyncBase.messages;

namespace TimeSyncBase.Connection
{
    public class StateObject
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
    }

    public abstract class ConnectionBase
    {
        protected abstract void HandleCorrectResponse(StateObject so, TimeSyncMessage message);
        protected bool TryBuildMessage(StateObject so, out TimeSyncMessage message)
        {
            message = MessageFactory.Build(so.sb.ToString());
            if (message == null)
                return false;
            return true;
        }
        protected bool TryReplyKnownProtocol(StateObject so)
        {
            TimeSyncMessage message;
            if (!TryBuildMessage(so, out message)) return false;
            HandleCorrectResponse(so, message);
            return true;
        }
    }
}