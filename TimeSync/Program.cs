using System;
using System.Linq;
using System.Threading;
using TimeSyncNodes;

namespace TimeSync
{
    internal class MainClass
    {
        private const string Server = "server";
        private const string Client = "client";
        private const int MillisecondsTimeout = 100;
        private static ETypeNode nodeType;
        private static INode _node;
        private static string _hostname;
        private static uint _port;

        public static void Main(string[] args)
        {
            if (!TryHandleArgs(args)) return;

            _node = NodeFactory.Build(nodeType, _hostname, _port);
            registerEvents(nodeType, _node);
            _node.StartService();
            LogScreenLoop();
            _node.StopService();
        }

        private static void registerEvents(ETypeNode eTypeNode, INode node)
        {
//            if (node is ClientNode)
//            {
//                ((ClientNode)node).
//            }
        }

        private static bool TryHandleArgs(string[] args)
        {
            return ValidadeFirstParam(args);
        }

        private static bool ValidadeFirstParam(string[] args)
        {
            if (args.Length > 0)
                switch (args[0].ToLower())
                {
                    case Server:
                        nodeType = ETypeNode.Server;
                        return ValidadeSecondParam(args);
                    case Client:
                        nodeType = ETypeNode.Client;
                        return ValidadeSecondParam(args);
                }
            Console.WriteLine("Tipo de nó incorreto ou não informado.");
            Console.WriteLine("Por favor digite 'Server' ou 'Client' como parametro. Ex:");
            Console.WriteLine("TimeSync client");
            return false;
        }

        private static bool ValidadeSecondParam(string[] args)
        {
            if (args.Length > 1 && args[0].ToLower() == Client)
            {
                _hostname = args[1];
                return ValidadeThirdParam(args);
            }
            if (args[0].ToLower() == Server)
            {
                if (args.Length > 1)
                    _port = Convert.ToUInt32(args[1]);
                return true;
            }

            Console.WriteLine("Nome do host remoto incorreto ou não informado.");
            Console.WriteLine("Por favor digite um IP ou dominio como parametro. Ex:");
            Console.WriteLine("TimeSync client 192.168.0.1 4781");
            return false;
        }

        private static bool ValidadeThirdParam(string[] args)
        {
            if (args.Length > 2)
            {
                _port = Convert.ToUInt32(args[2]);
                return true;
            }

            Console.WriteLine("Porta do host remoto incorreto ou não informado.");
            Console.WriteLine("Por favor digite uma Porta como parametro. Ex:");
            Console.WriteLine("TimeSync client 192.168.0.1 4781");
            return false;
        }

        private static void LogScreenLoop()
        {
            ConsoleKey consoleKey;
            do
            {
                while (!Console.KeyAvailable)
                {
                    PrintResumeScreen();
                    Thread.Sleep(MillisecondsTimeout);
                }
                consoleKey = Console.ReadKey(true).Key;
                TreatSpecialKeys(consoleKey);
            } while (consoleKey != ConsoleKey.Escape);
        }

        private static void TreatSpecialKeys(ConsoleKey consoleKey)
        {
            if (_node is ServerNode)
            {
                handleServerSpecialKeys(consoleKey);
            }
        }

        private static void handleServerSpecialKeys(ConsoleKey consoleKey)
        {
            var serverNode = ((ServerNode)_node);
            switch (consoleKey)
            {
                case ConsoleKey.Add:
                    serverNode.UpdateDateTimeServer(DateTime.UtcNow.AddSeconds(10));
                    break;
                case ConsoleKey.Subtract:
                    serverNode.UpdateDateTimeServer(DateTime.UtcNow.AddSeconds(-10));
                    break;
            }
        }

        private static void PrintResumeScreen()
        {
            Console.Clear();
            foreach (var ipNode in _node.GetActiveConnections())
            {
                Console.WriteLine("{0} | {1} | {2}", ipNode.GetIP(), ipNode.GetPort(), ipNode.GetLocalTime().GetDateTime());
            }
        }
    }
}