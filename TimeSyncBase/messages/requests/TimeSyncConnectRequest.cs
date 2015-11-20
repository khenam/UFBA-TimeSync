using System;

namespace TimeSyncBase.messages.requests
{
    public class TimeSyncConnectRequest : TimeSyncMessage
    {
        public TimeSyncConnectRequest() : base((int) ETimeSyncMessageTypes.TimeSyncConnectRequest)
        {
        }

        public uint NewConnectionPort { get; set; }
    }
}