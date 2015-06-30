using TimeSyncBase.messages;

namespace TimeSyncBase.Connection
{
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

        public const uint DefaultPort = 4781;
    }
}