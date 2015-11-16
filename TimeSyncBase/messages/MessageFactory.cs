using System;
using Newtonsoft.Json;
using TimeSyncBase.messages;
using TimeSyncBase.messages.requests;
using TimeSyncBase.messages.responseless;
using TimeSyncBase.messages.responses;

namespace TimeSyncBase.messages
{
    public class MessageFactory
    {
        public static TimeSyncMessage Build(string message)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(message);
                switch ((ETimeSyncMessageTypes) json.IdMessage)
                {
                    case ETimeSyncMessageTypes.TimeSyncConnectRequest:
                        return JsonConvert.DeserializeObject<TimeSyncConnectRequest>(message);
                    case ETimeSyncMessageTypes.TimeSyncConnectResponse:
                        return JsonConvert.DeserializeObject<TimeSyncConnectResponse>(message);
                    case ETimeSyncMessageTypes.TimeSyncRequest:
                        return JsonConvert.DeserializeObject<TimeSyncRequest>(message);
                    case ETimeSyncMessageTypes.TimeSyncResponse:
                        return JsonConvert.DeserializeObject<TimeSyncResponse>(message);
                    case ETimeSyncMessageTypes.TimeSyncSimpleRequest:
                        return JsonConvert.DeserializeObject<TimeSyncSimpleRequest>(message);
                    case ETimeSyncMessageTypes.TimeSyncSimpleResponse:
                        return JsonConvert.DeserializeObject<TimeSyncSimpleResponse>(message);
                    case ETimeSyncMessageTypes.TimeSyncConnectedClientsRequest:
                        return JsonConvert.DeserializeObject<TimeSyncConnectedClientsRequest>(message);
                    case ETimeSyncMessageTypes.TimeSyncConnectedClientsResponse:
                        return JsonConvert.DeserializeObject<TimeSyncConnectedClientsResponse>(message);
                    case ETimeSyncMessageTypes.TimeSyncResponseless:
                        return JsonConvert.DeserializeObject<TimeSyncResponseless>(message);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}