using System;

namespace TimeSyncBase
{
    public class Calculator
    {
        public static DateTime PullTimeSyncCalc(DateTime LocalSendTime, DateTime RemoteReceiveTime, DateTime Time,
            DateTime LocalResponseTime)
        {
            var timaSpanGeneral = (LocalResponseTime.Subtract(LocalSendTime));
            var timeSpanRemote = (Time.Subtract(RemoteReceiveTime));
            var delay = new TimeSpan(timaSpanGeneral.Subtract(timeSpanRemote).Ticks/2);
            return Time.Add(delay);
        }
    }
}