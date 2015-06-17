using System;
using Newtonsoft.Json;

namespace TimeSyncBase.messages
{
	abstract class TimeSyncMessage
	{
		public abstract const IdMessage;
		public virtual string ToJSON()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}

