using System;

namespace TimeSyncBase.messages.responses
{
	public class TimeSyncConnectedClientsResponse
	{
		public const int IdMessage = 4;
		public string[] ClientsIps;
		public TimeSyncConnectedClientsResponse ()
		{
		}
	}
}

