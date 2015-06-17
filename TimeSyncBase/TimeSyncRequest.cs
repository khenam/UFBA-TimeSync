using System;
using TimeSyncBase.messages;

namespace TimeSyncBase.messages.requests
{
	public class TimeSyncRequest : TimeSyncMessage
	{
		public const int IdMessage = 1;
		public DateTime RequestTime { get; set;}
		public TimeSyncRequest ()
		{
		}
	}
}

