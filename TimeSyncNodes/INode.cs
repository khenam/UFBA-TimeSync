using System;
using System.Collections.Generic;
using System.Net;
using TimeSyncBase;
using TimeSyncBase.Connection;

namespace TimeSync
{
    public interface INode
    {
        IPAddress IpAddress { get; set; }
        uint Port { get; set; }
        bool IsRunning { get; }
        bool StartService();
        void StopService();
        List<IPAddress> GetActiveIPs();
        List<ConnectionBase> GetActiveConnections();
        List<NodeReference> GetActiveConnectionsNodes();
        
        DateTime GetDateTime(bool localtime = true);
    }
}