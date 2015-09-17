namespace TimeSyncBase.messages.requests
{
    public class TimeSyncConnectedClientsRequest : TimeSyncMessage
    {
        public TimeSyncConnectedClientsRequest()
            : base((int) ETimeSyncMessageTypes.TimeSyncConnectedClientsRequest)
        {
        }
    }
}