using System;
using System.Linq;
using System.Threading;

namespace TimeSync
{
	class MainClass
	{
	    private static ETypeNode nodeType;
	    private static INode _node;
	    private static string _hostname;

	    public static void Main (string[] args)
		{
		    if (!TryHandleArgs(args)) return ;

            _node = NodeFactory.Build(nodeType, _hostname);

	        _node.StartService();
	        LogScreenLoop();
            _node.StopService();
		}

	    private static bool TryHandleArgs(string[] args)
	    {
	        return ValidadeFirstParam(args)&&ValidadeSecondParam(args);
	    }

	    private static bool ValidadeSecondParam(string[] args)
	    {
	        if (args.Length > 1)
	        {
	            _hostname = args[1];
	            return true;
	        }
                
            Console.WriteLine("Nome do host remoto incorreto ou não informado.");
            Console.WriteLine("Por favor digite um IP ou dominio como parametro. Ex:");
            Console.WriteLine("TimeSync client 192.168.0.1");
            return false;
	    }

	    private static bool ValidadeFirstParam(string[] args)
	    {
	        if (args.Length > 0)
	            switch (args[0].ToLower())
	            {
	                case "server":
	                    nodeType = ETypeNode.Server;
	                    return true;
	                case "client":
	                    nodeType = ETypeNode.Client;
	                    return true;
	            }
	        Console.WriteLine("Tipo de nó incorreto ou não informado.");
	        Console.WriteLine("Por favor digite 'Server' ou 'Client' como parametro. Ex:");
	        Console.WriteLine("TimeSync client");
	        return false;
	    }

	    private static void LogScreenLoop()
	    {
	        do
	        {
	            PrintResumeScreen();
                Thread.Sleep(300);
	        } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
	    }

	    private static void PrintResumeScreen()
	    {
	        Console.Clear();
	        foreach (var ipNode in _node.GetActiveConnections())
	        {
                Console.WriteLine(string.Join(".", ipNode.GetAddressBytes().Select(a => a.ToString("d"))));
	        }
            
	    }
	}
}
