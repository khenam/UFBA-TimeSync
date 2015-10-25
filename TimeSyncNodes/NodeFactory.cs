using System;
using TimeSyncNodes;

namespace TimeSync
{
    public class NodeFactory
    {
        public static INode Build(ETypeNode eTypeNode, string hostNAme = "server.timesync.ufba.br", uint? port = null)
        {
            switch (eTypeNode)
            {
                case ETypeNode.Server:
                    return port.HasValue? new ServerNode(port.Value) : new ServerNode();
                case ETypeNode.Client:
                    return port.HasValue ? new ClientNode(hostNAme,port.Value) : new ClientNode(hostNAme);
                default:
                    throw new Exception("Tipo de Nó Desconhecido.");
            }
        }
    }
}