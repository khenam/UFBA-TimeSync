using Newtonsoft.Json;

namespace TimeSyncBase.messages
{
    public abstract class TimeSyncMessage
    {
        private readonly int _idMesnagem;

        protected TimeSyncMessage(int idMesnagem)
        {
            _idMesnagem = idMesnagem;
        }

        public int IdMessage
        {
            get { return _idMesnagem; }
        }

        public virtual string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}