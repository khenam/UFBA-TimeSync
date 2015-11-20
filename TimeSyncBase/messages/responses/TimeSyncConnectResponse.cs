using System;

namespace TimeSyncBase.messages.responses
{
    public class TimeSyncConnectResponse : TimeSyncMessage
    {
        public TimeSyncConnectResponse()
            : base((int) ETimeSyncMessageTypes.TimeSyncConnectResponse)
        {
        }

        public int ReturnCode { get; set; }
    }
}