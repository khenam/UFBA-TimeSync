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
        private TimeSpan _oneSecond = new TimeSpan(0, 0, 1);
        private TimeSpan _twoSeconds = new TimeSpan(0, 0, 2);
        private TimeSpan _4Seconds = new TimeSpan(0, 0, 4);
        private TimeSpan _5Seconds = new TimeSpan(0, 0, 5);

        [Test]
        public void CalcZeroTimeStamp()
        {
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, _localSendTime, _localSendTime, _localSendTime);
            Assert.That(CalculatedDate, Is.EqualTo(_localSendTime));
        }
        [Test]
        public void CalcOneSecondTimeStamp()
        {
            DateTime timePlusOne = _localSendTime.Add(_oneSecond);
            DateTime timePlusTwo = _localSendTime.Add(_twoSeconds);
            DateTime serverTime = timePlusOne;
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusTwo);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_oneSecond)));
        }
        [Test]
        public void CalcTwoSecondsTimeStamp()
        {
            DateTime timePlusOne = _localSendTime.Add(_oneSecond);
            DateTime timePlusFour = _localSendTime.Add(_4Seconds);
            DateTime serverTime = timePlusOne;
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusFour);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_twoSeconds)));
        }
        [Test]
        public void CalcTwoSecondsUnbalancedTimeStamp()
        {
            DateTime timePlusOne = _localSendTime.Add(_oneSecond);
            DateTime timePlusFive = _localSendTime.Add(_5Seconds);
            DateTime serverTime = timePlusOne.Add(_oneSecond);
            var CalculatedDate = Calculator.PullTimeSyncCalc(_localSendTime, timePlusOne, serverTime, timePlusFive);
            Assert.That(CalculatedDate, Is.EqualTo(serverTime.Add(_twoSeconds)));
        }

        [Test]
        public void LocalTimeTimeStampValidation()
        {
            LocalTime localTime = new LocalTime(_localSendTime);
            localTime.SetDateTime(DateTime.Now.Add(_5Seconds));
			Assert.That(Math.Round(localTime.GetTimeSpan().TotalMilliseconds,0), Is.EqualTo(_5Seconds.TotalMilliseconds));
            Thread.Sleep(1000);
			Assert.That(localTime.GetDateTime().ToString("yyyy-MM-dd hh:mm:ss"), Is.EqualTo(DateTime.Now.Add(_5Seconds).ToString("yyyy-MM-dd hh:mm:ss")));
        }
    }
}