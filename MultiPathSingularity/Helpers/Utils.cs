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

        public static void PrintRouteStates(List<Route> routes, bool clear)
        {
            if (clear)
                ClearLastNLines(routes.Count + 1);

            Console.WriteLine($"Routes: {routes.Count}");
            foreach (Route r in routes)
            {
                Console.WriteLine($"{r.IPAddress}:{r.Port} - [{r.Latency}] [{r.LastPing}]");
            }
        }


        private static void ClearLastNLines(int n)
        {
            int cursorTop = Console.CursorTop; // Store the current cursor position
            int newCursorPosition = Math.Max(0, cursorTop - n); // Calculate the new cursor position

            // Set the cursor position to the new line
            Console.SetCursorPosition(0, newCursorPosition);

            // Clear the desired number of lines by overwriting them with empty spaces
            for (int i = 0; i < n; i++)
            {
                Console.WriteLine(new string(' ', Console.BufferWidth - 1));
            }

            // Set the cursor position back to where you want to start printing the new content
            Console.SetCursorPosition(0, newCursorPosition);
        }
    }
}
