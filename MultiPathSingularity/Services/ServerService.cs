﻿using MultiPathSingularity.Helpers;
using MultiPathSingularity.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiPathSingularity.Services
{
    public static class ServerService
    {
        private static Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>? routes = new Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>();
        private static UdpClient fwClient = new UdpClient(0);
        private static UdpClient bckClient = new UdpClient(0);
        public static void StartServer(string port, string destination)
        {
            Console.WriteLine($"Starting server on port {port}... ");

            _ = Task.Run(() =>
                    FwService(int.Parse(port), destination)
                );


            _ = Task.Run(() =>
                    BckService()
                );

            if (routes == null)
                routes = new Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>();

            Utils.PrintRouteStates(routes.Keys.ToList() ?? new List<Route>(), false);
            while (true)
            {
                Utils.PrintRouteStates(routes.Keys.ToList(), true);
                Thread.Sleep(1500);

                //Clear routes that have been inactive for more than 5 seconds
                foreach(Route r in routes.Keys.ToList())
                {
                    if((DateTime.UtcNow - r.LastPing).TotalSeconds > 5)
                    {
                        routes.Remove(r);
                    }
                }
            }
        }

        //Received from MP Client to Server
        private static void FwService(int port, string destination)
        {
            Console.WriteLine("[Server] FwService is running...");

            fwClient = new UdpClient(port);

            //Route and queue to send packets received from MP Client directly to Server
            BlockingCollection<(byte[], UdpClient?)> queue = new BlockingCollection<(byte[], UdpClient?)>();
            Route? route = Utils.ReadRoute(destination, queue);

            if(route == null)
            {
                Console.WriteLine("Missing destination route.\nUsage: mpsingularity server <PORT> \"1.2.3.4:1234\"");
                Environment.Exit(11);
            }

            //Initialize routes - Error check for null
            if (routes == null)
                routes = new Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>();

            while (true)
            {
                try
                {
                    IPEndPoint _loopback = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = fwClient.Receive(ref _loopback);
                                            
                    //Any new packet received should be registered to be used a route to send packets back through
                    Route _route = new Route() { IPAddress = _loopback.Address, Port = _loopback.Port };
                    if (!routes.ContainsKey(_route))
                    {
                        BlockingCollection<(byte[], UdpClient?)>? _queue = new BlockingCollection<(byte[], UdpClient?)>();
                        routes.Add(new Route(_queue, null, fwClient) { IPAddress = _loopback.Address, Port = _loopback.Port }, _queue);
                    }

                    //Whenever clients enables/disables a route, a packet with 3 bytes is sent to this route letting the route know what state it should take
                    if (data.Length == 3)
                    {
                        Route? r = routes.Keys.FirstOrDefault(r => r.Equals(_route));
                        if (r == null)
                            continue;

                        r.SetRouteActive(data[2]);

                        continue;
                    }

                    //Ping Packets will be bounced back - Wireguard Packets (and most regular packets) will always be larger than 2 byte
                    if (data.Length == 2)
                    {
                        fwClient.Send(data.Take(1).ToArray(), 1, _loopback);
                        continue;
                    }

                    if(data.Length == 1)
                    {
                        Route? r = routes.Keys.FirstOrDefault(r => r.Equals(_route));
                        if (r == null)
                            continue;
                        
                        r.CalculateLatency(data[0]);
                    }

                    //Use Destination queue to finish delivery of packet to Server
                    queue.Add((data, bckClient));
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        //Received from Server sent back to MP Client
        private static void BckService()
        {
            Console.WriteLine("[Server] BckService is running...");

            //Initialize routes - Error check for null
            if (routes == null)
                routes = new Dictionary<Route, BlockingCollection<(byte[], UdpClient?)>>();

            while (true)
            {
                try
                {
                    IPEndPoint _loopback = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = bckClient.Receive(ref _loopback);

                    //Send packet back through all routes that have connected to server
                    foreach (var route in routes)
                    {
                        route.Value.Add((data, fwClient));
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }
    }
}
