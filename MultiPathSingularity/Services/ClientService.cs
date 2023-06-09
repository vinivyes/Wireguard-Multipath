﻿using MultiPathSingularity.Helpers;
using MultiPathSingularity.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiPathSingularity.Services
{
    public static class ClientService
    {
        public static BlockingCollection<byte[]> bckQueue = new BlockingCollection<byte[]>();
        private static Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>> routes = new Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>();
        private static IPEndPoint? _bwEndpoint = null;
        private static UdpClient fwClient = new UdpClient(0);

        public static void StartClient(string port, string routesFile)
        {
            Console.WriteLine($"Starting client on port {port}...");

            if (!File.Exists(routesFile))
            {
                Console.WriteLine("Missing routes file.\nUsage: mpsingularity client <PORT> \"./routes.txt\"\n\nThe contents of 'routes.txt' should look as follows:\n1.2.3.4:1234\n2.3.4.5:2345");
                Environment.Exit(10);
            }
            else
            {
                string[] _routes = File.ReadAllLines(routesFile);
                foreach (string route in _routes)
                {
                    BlockingCollection<(byte[], UdpClient?)> queue = new BlockingCollection<(byte[], UdpClient?)>();
                    Route? _route = Utils.ReadRoute(route, queue, bckQueue);

                    if (_route != null)
                        routes.Add(_route, queue);
                }
            }

            _ = Task.Run(() =>
                    BckService()
                );

            _ = Task.Run(() =>
                    FwService(int.Parse(port))
                );

            Utils.PrintRouteStates(routes.Keys.ToList() ?? new List<Route>(), false);
            while (true)
            {
                Utils.PrintRouteStates(routes.Keys.ToList(), true);
                Thread.Sleep(1500);
            }
        }

        //Received from Client forwarded to MP Server
        private static void FwService(int port)
        {
            Console.WriteLine("[Client] FwService is running...");
            fwClient = new UdpClient(port);

            while (true)
            {
                try
                {
                    byte[] data = fwClient.Receive(ref _bwEndpoint);

                    //Add data to the queue of all routes
                    foreach (var route in routes)
                    {
                        //Forward the item using the UdpClient that will be expecting a response back
                        route.Value.Add((data, null));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        //Received from MP Server back to Client
        private static void BckService()
        {
            Console.WriteLine("[Client] BckService is running...");
            while (true)
            {
                try
                {
                    // Wait for data to become available
                    var data = bckQueue.Take();

                    //Send received packets back to client using UdpClient that receives packets
                    if (_bwEndpoint != null)
                        fwClient.Send(data, data.Length, _bwEndpoint);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
