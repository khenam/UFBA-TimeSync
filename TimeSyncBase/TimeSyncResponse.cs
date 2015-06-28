using System;

namespace TimeSyncBase.messages.responses
{
	public class TimeSyncResponse : TimeSyncMessage
	{
		public DateTime RequestTime { get; set;}
		public DateTime ReceivedTime { get; set;}
		public DateTime ResponseTime { get; set;}
        public TimeSyncResponse()
            : base((int)ETimeSyncMessageTypes.TimeSyncResponse)
		{
		}
	}
}

