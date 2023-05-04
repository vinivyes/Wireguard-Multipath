using MultiPathSingularity.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiPathSingularity.Helpers
{
    public static class Utils
    {
        public static Route? ReadRoute(string route, BlockingCollection<(byte[], UdpClient?)>? queue, BlockingCollection<byte[]>? _bckQueue = null)
        {
            string ipAddressPattern = @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";
            string portPattern = @"(?<=:)([0-9]{1,5})";

            Regex ipAddressRegex = new Regex(ipAddressPattern);
            Regex portRegex = new Regex(portPattern);

            Match ipAddressMatch = ipAddressRegex.Match(route);
            Match portMatch = portRegex.Match(route);

            if (ipAddressMatch.Success && portMatch.Success)
            {
                string ipAddress = ipAddressMatch.Value;
                int port = int.Parse(portMatch.Value);

                if (queue == null)
                    return null;

                if (port >= 0 && port <= 65535)
                {
                    return new Route(queue ?? new BlockingCollection<(byte[], UdpClient?)>(), _bckQueue) { IPAddress = IPAddress.Parse(ipAddress), Port = port };
                }
                else
                {
                    Console.WriteLine($"Route: {route} [Invalid Port]");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"Route: {route} [Invalid Route]");
                return null;
            }
        }
    }
}
