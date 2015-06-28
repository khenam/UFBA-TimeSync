using System;
using Newtonsoft.Json;

namespace TimeSyncBase.messages
{
    public enum ETimeSyncMessageTypes
    {
        TimeSyncRequest = 1,
        TimeSyncResponse,
        TimeSyncConnectedClientsRequest,
        TimeSyncConnectedClientsResponse
    } 
    public abstract class TimeSyncMessage
	{
        protected TimeSyncMessage(int idMesnagem)
        {
            _idMesnagem = idMesnagem;
        }

        private readonly int _idMesnagem;
        public int IdMessage { get { return _idMesnagem; } }
		public virtual string ToJSON()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}