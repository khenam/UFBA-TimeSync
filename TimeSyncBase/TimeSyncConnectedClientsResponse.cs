using System;

namespace TimeSyncBase.messages.responses
{
    public class TimeSyncConnectedClientsResponse : TimeSyncMessage
	{
		public string[] ClientsIps;
        public TimeSyncConnectedClientsResponse()
            : base((int)ETimeSyncMessageTypes.TimeSyncConnectedClientsResponse)
		{
		}
	}
}

