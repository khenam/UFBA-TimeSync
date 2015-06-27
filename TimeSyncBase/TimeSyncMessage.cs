using System;
using Newtonsoft.Json;

namespace TimeSyncBase.messages
{
    public abstract class TimeSyncMessage
	{
		public virtual string ToJSON()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}