using System;
using System.Threading;
using NUnit.Framework;
using TimeSyncBase;

namespace TestTimeSync
{
    [TestFixture]
    public class TestCalcTimeSync
    {
        private DateTime _localSendTime = DateTime.Now;
        private readonly TimeSpan _oneSecond = new TimeSpan(0, 0, 1);
        private readonly TimeSpan _twoSeconds = new TimeSpan(0, 0, 2);
        private readonly TimeSpan _4Seconds = new TimeSpan(0, 0, 4);
        private TimeSpan _5Seconds = new TimeSpan(0, 0, 5);

        [Test]
        public void CalcOneSecondTimeStamp()
        {
            var timePlusOne = _localSendTime.Add(_oneSecond);
            var timePlusTwo = _localSendTime.Add(_twoSeconds);
            var serverTime = timePlusOne;
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusTwo);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_oneSecond)));
        }

        [Test]
        public void CalcTwoSecondsTimeStamp()
        {
            var timePlusOne = _localSendTime.Add(_oneSecond);
            var timePlusFour = _localSendTime.Add(_4Seconds);
            var serverTime = timePlusOne;
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusFour);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_twoSeconds)));
        }

        [Test]
        public void CalcTwoSecondsUnbalancedTimeStamp()
        {
            var timePlusOne = _localSendTime.Add(_oneSecond);
            var timePlusFive = _localSendTime.Add(_5Seconds);
            var serverTime = timePlusOne.Add(_oneSecond);
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusFive);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_twoSeconds)));
        }

        [Test]
        public void CalcZeroTimeStamp()
        {
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, _localSendTime, _localSendTime,
                _localSendTime);
            Assert.That(CalculatedDate, Is.EqualTo(_localSendTime));
        }

        [Test]
        public void LocalTimeTimeStampValidation()
        {
            var localTime = new LocalTime(_localSendTime);
            localTime.SetDateTime(DateTime.Now.Add(_5Seconds));
            Assert.That(Math.Round(localTime.GetTimeSpan().TotalMilliseconds, 0),
                Is.EqualTo(_5Seconds.TotalMilliseconds));
            Thread.Sleep(1000);
            Assert.That(localTime.GetDateTime().ToString("yyyy-MM-dd hh:mm:ss"),
                Is.EqualTo(DateTime.Now.Add(_5Seconds).ToString("yyyy-MM-dd hh:mm:ss")));
        }
    }
}