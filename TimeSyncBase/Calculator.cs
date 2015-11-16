using System;

namespace TimeSyncBase
{
    public class Calculator
    {
        public static DateTime PullTimeSyncCalc(DateTime localSendTime, DateTime remoteReceiveTime, DateTime time,
            DateTime localResponseTime)
        {
            var timeSpanGeneral = (localResponseTime.ToUniversalTime().Subtract(localSendTime.ToUniversalTime()));
            var timeSpanRemote = (time.ToUniversalTime().Subtract(remoteReceiveTime.ToUniversalTime()));
            var delay = new TimeSpan(timeSpanGeneral.Subtract(timeSpanRemote).Ticks/2);
            return time.Add(delay);
        }

        public static DateTime PullTimeSyncCalc(DateTime localSendTime, DateTime time, DateTime localResponseTime)
        {
            var timeSpanGeneral = (localResponseTime.ToUniversalTime().Subtract(localSendTime.ToUniversalTime()));
            var delay = new TimeSpan(timeSpanGeneral.Ticks / 2);
            return time.Add(delay);
        }
    }
}