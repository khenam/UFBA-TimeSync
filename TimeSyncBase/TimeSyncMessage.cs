using System;
using Newtonsoft.Json;

namespace TimeSyncBase.messages
{
	abstract class TimeSyncMessage
	{
		public virtual string ToJSON()
		{
			return JsonConvert.SerializeObject (this);
		}
	}
}

