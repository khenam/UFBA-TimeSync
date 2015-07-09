using System;
using System.Collections.Generic;
using System.Net;

namespace TimeSync
{
    internal interface INode
    {
        IPAddress IpAddress { get; set; }
        uint Port { get; set; }
        bool IsRunning { get; }
        bool StartService();
        void StopService();
        List<IPAddress> GetActiveConnections();
        DateTime GetDateTime(bool localtime = true);
    }
}