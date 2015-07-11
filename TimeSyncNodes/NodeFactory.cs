using System;

namespace TimeSync
{
    public class NodeFactory
    {
        public static INode Build(ETypeNode eTypeNode, string hostNAme = "server.timesync.ufba.br")
        {
            switch (eTypeNode)
            {
                case ETypeNode.Server:
                    return new ServerNode();
                case ETypeNode.Client:
                    return new ClientNode(hostNAme);
                default:
                    throw new Exception("Tipo de Nó Desconhecido.");
            }
        }
    }
}