using MultiPathSingularity.Services;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace MultiPathSingularity;
class Program
{

    static void Main(string[] args)
    {
        LoadArgs(args);
    }
    public static void LoadArgs(string[] args)
    {
        for(int a = 0; a < args.Length; a++)
        {
            switch (args[a])
            {
                case "server":
                    ServerService.StartServer(args[a + 1], args[a + 2]);
                    a = a + 2;
                    break;
                case "client":
                    ClientService.StartClient(args[a + 1], args[a + 2]);
                    a = a + 2;
                    break;
            }
        }
    }
}