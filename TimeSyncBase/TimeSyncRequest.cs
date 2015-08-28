using System;

namespace TimeSyncBase.messages.requests
{
    public class TimeSyncRequest : TimeSyncMessage
    {
        public TimeSyncRequest() : base((int) ETimeSyncMessageTypes.TimeSyncRequest)
        {
        }

        public DateTime RequestTime { get; set; }
    }
}