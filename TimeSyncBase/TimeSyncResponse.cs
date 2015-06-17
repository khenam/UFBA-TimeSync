using System;

namespace TimeSyncBase
{
	public class TimeSyncResponse
	{
		public DateTime RequestTime { get; set;}
		public DateTime ReceivedTime { get; set;}
		public DateTime ResponseTime { get; set;}
		public TimeSyncResponse ()
		{
		}
	}
}

