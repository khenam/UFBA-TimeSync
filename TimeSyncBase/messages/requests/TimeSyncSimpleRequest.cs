using System;

namespace TimeSyncBase.messages.requests
{
    public class TimeSyncSimpleRequest : TimeSyncMessage
    {
        public TimeSyncSimpleRequest() : base((int) ETimeSyncMessageTypes.TimeSyncSimpleRequest)
        {
        }

        public DateTime RequestTime { get; set; }
    }
}