using System;

namespace TimeSyncBase.messages.responses
{
    public class TimeSyncSimpleResponse : TimeSyncMessage
    {
        public TimeSyncSimpleResponse()
            : base((int) ETimeSyncMessageTypes.TimeSyncSimpleResponse)
        {
        }

        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
    }
}