using System;

namespace TimeSyncBase
{
    public class LocalTime : ICloneable
    {
        private TimeSpan _timeSpan;

        public LocalTime()
        {
            SetDateTime(DateTime.UtcNow);
        }

        public LocalTime(DateTime time)
        {
            SetDateTime(time);
        }

        public DateTime GetDateTime()
        {
            return DateTime.UtcNow.Add(_timeSpan);
        }
        public DateTime GetLocalDateTime()
        {
            return DateTime.Now.Add(_timeSpan);
        }

        public void SetDateTime(DateTime newTime)
        {
            _timeSpan = -DateTime.UtcNow.Subtract(newTime);
        }

        public TimeSpan GetTimeSpan()
        {
            return _timeSpan;
        }

        public void SetTimeSpan(TimeSpan timeSpan)
        {
            _timeSpan = timeSpan;
        }

        public object Clone()
        {
            return new LocalTime(){_timeSpan = _timeSpan};
        }
    }
}