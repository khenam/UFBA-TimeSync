using System;
using System.Net;

namespace TimeSyncBase
{
    public class Calculator
    {
        public static DateTime PullTimeSyncCalc(DateTime LocalSendTime, DateTime RemoteReceiveTime, DateTime Time, DateTime LocalResponseTime)
        {
            TimeSpan timaSpanGeneral = (LocalResponseTime.Subtract(LocalSendTime));
            TimeSpan timeSpanRemote = (Time.Subtract(RemoteReceiveTime));
            TimeSpan delay = new TimeSpan(timaSpanGeneral.Subtract(timeSpanRemote).Ticks/2);
            return Time.Add(delay);
        }
    }
}