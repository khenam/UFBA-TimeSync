using System;

namespace TimeSyncBase.messages.responseless
{
    public class TimeSyncResponseless : TimeSyncMessage
    {
        public TimeSyncResponseless()
            : base((int) ETimeSyncMessageTypes.TimeSyncResponseless)
        {
        }

        public DateTime ResponseTime { get; set; }
    }
}