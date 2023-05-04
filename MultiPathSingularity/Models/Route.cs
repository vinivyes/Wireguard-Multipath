using MultiPathSingularity.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MultiPathSingularity.Models
{
    public class Route
    {
        public Route()
        {

        }

        public Route(BlockingCollection<(byte[], UdpClient?)>? queue, BlockingCollection<byte[]>? bckQueue = null, UdpClient? udpClient = null)
        {
            if (queue != null)
                Task.Run(() => WorkerThread(queue));
            
            if(bckQueue != null)
                Task.Run(() => UdpThread(bckQueue));

            //If any task is running, start latency thread
            if (bckQueue != null || queue != null)
                Task.Run(() => LatencyThread(udpClient ?? _routeUdp));
        }

        public IPAddress IPAddress { get; set; } = new IPAddress(0);
        public int Port { get; set; }
        public double Latency { get; set; } = 999;
        public DateTime LastPing { get; set; } = DateTime.UtcNow;

        private byte latencyIdx = 0;
        private DateTime latencyStart = DateTime.UtcNow;

        private UdpClient _routeUdp = new UdpClient(0);

        private void WorkerThread(BlockingCollection<(byte[], UdpClient?)> queue)
        {
            if (IPAddress == null)
                return;

            while (true)
            {
                // Wait for data to become available
                var (data, udp) = queue.Take();

                //Send through specified client or through route udp
                (udp ?? _routeUdp).Send(data, data.Length, new IPEndPoint(IPAddress, Port));
            }
        }

        private void UdpThread(BlockingCollection<byte[]> bckQueue)
        {
            while (true)
            {
                IPEndPoint _loopback = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _routeUdp.Receive(ref _loopback);

                //Ping Packets will be bounced back - Wireguard Packets (and most regular packets) will always be larger than 2 byte
                if (data.Length == 2)
                {
                    _routeUdp.Send(data.Take(1).ToArray(), 1, _loopback);
                    continue;
                }

                if (data.Length == 1)
                {
                    CalculateLatency(data[0]);
                    continue;
                }

                bckQueue.Add(data);
            }
        }

        private void LatencyThread(UdpClient client)
        {
            while (true)
            {
                client.Send(new byte[] { ++latencyIdx, 0 }, 2, new IPEndPoint(IPAddress, Port));
                latencyStart = DateTime.UtcNow;

                Thread.Sleep(1500);
            }
        }

        public void CalculateLatency(byte idx)
        {
            if (idx != latencyIdx)
                return;

            Latency = (DateTime.UtcNow - latencyStart).TotalMilliseconds;
            LastPing = DateTime.UtcNow;
        }

        //The following 2 methods (Equals and GetHashCode) ensure that this class can be compared using IPAddress and Port only ignoring everything else.
        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Route other = (Route)obj;
            return IPAddress.Equals(other.IPAddress) && Port == other.Port;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + (IPAddress?.GetHashCode() ?? 0);
            hash = hash * 23 + Port.GetHashCode();
            return hash;
        }
    }
}
