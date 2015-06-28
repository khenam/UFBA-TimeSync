using System;
using TimeSyncBase.messages;

namespace TimeSyncBase.messages.requests
{
	public class TimeSyncRequest : TimeSyncMessage
	{
		public DateTime RequestTime { get; set;}
		public TimeSyncRequest ():base((int) ETimeSyncMessageTypes.TimeSyncRequest)
		{
		}
	}
}

