using System;

namespace TimeSyncBase.messages.responses
{
	public class TimeSyncResponse : TimeSyncMessage
	{
		public const int IdMessage = 2;
		public DateTime RequestTime { get; set;}
		public DateTime ReceivedTime { get; set;}
		public DateTime ResponseTime { get; set;}
		public TimeSyncResponse ()
		{
		}
	}
}

