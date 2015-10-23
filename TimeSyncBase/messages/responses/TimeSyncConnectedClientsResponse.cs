using TimeSyncBase;
namespace TimeSyncBase.messages.responses
{
    public class TimeSyncConnectedClientsResponse : TimeSyncMessage
    {
        public NodeReference[] ClientsIps;

        public TimeSyncConnectedClientsResponse()
            : base((int) ETimeSyncMessageTypes.TimeSyncConnectedClientsResponse)
        {
        }
    }
}