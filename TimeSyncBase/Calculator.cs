using System;

namespace TimeSyncBase
{
    public class Calculator
    {
        public static DateTime PullTimeSyncCalc(DateTime localSendTime, DateTime remoteReceiveTime, DateTime time,
            DateTime localResponseTime)
        {
            var timeSpanGeneral = (localResponseTime.Subtract(localSendTime));
            var timeSpanRemote = (time.Subtract(remoteReceiveTime));
            var delay = new TimeSpan(timeSpanGeneral.Subtract(timeSpanRemote).Ticks/2);
            return time.Add(delay);
        }
    }
}