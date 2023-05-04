using MultiPathSingularity.Services;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MultiPathSingularity;
class Program
{

    static void Main(string[] args)
    {
        int services = LoadArgs(args);

        //Keep main thread running if services were loaded from Arguments
        while(services > 0)
        {
            Thread.Sleep(100);
        }
    }
    public static int LoadArgs(string[] args)
    {
        //How many services were loaded
        int s = 0;

        for(int a = 0; a < args.Length; a++)
        {
            switch (args[a])
            {
                case "server":
                    if (args.Length < a + 2)
                    {
                        Console.WriteLine("Server is missing arguments.\nUsage: mpsingularity server <PORT> \"1.2.3.4:1234\"");
                        Environment.Exit(12);
                    }
                    
                    ServerService.StartServer(args[a + 1], args[a + 2]);
                    a = a + 2;
                    s++;
                    break;
                case "client":
                    if (args.Length < a + 2)
                    {
                        Console.WriteLine("Client is missing arguments.\nUsage: mpsingularity client <PORT> \"./routes.txt\"\n\nThe contents of 'routes.txt' should look as follows:\n1.2.3.4:1234\n2.3.4.5:2345");
                        Environment.Exit(13);
                    }

                    ClientService.StartClient(args[a + 1], args[a + 2]);
                    a = a + 2;
                    s++;
                    break;
            }
        }

        //If no services were loaded
        if (args.Length == 0 || s == 0)
        {
            Console.WriteLine("\nMissing arguments, here is how to use:\n\n---------------------\nAs a Server:\n---------------------\nmpsingularity server<PORT> \"1.2.3.4:1234\"\n\n---------------------\nAs a Client:\n---------------------\nmpsingularity client <PORT> \"./routes.txt\"\n\nThe contents of 'routes.txt' should look as follows:\n1.2.3.4:1234\n2.3.4.5:2345");
            Environment.Exit(14);
        }

        return s;
    }
}