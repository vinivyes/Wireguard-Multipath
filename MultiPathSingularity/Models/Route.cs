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

        public Route(BlockingCollection<(byte[], UdpClient)> queue)
        {
            Task.Run(() => WorkerThread(queue));
        }

        public IPAddress IPAddress { get; set; } = new IPAddress(0);
        public int Port { get; set; }
        public double Latency { get; set; }

        private void WorkerThread(BlockingCollection<(byte[], UdpClient)> queue)
        {
            if (IPAddress == null)
                return;

            while (true)
            {
                // Wait for data to become available
                var (data, udp) = queue.Take();

                udp.Send(data, data.Length, new IPEndPoint(IPAddress, Port));
            }
        }

        //The following 2 methods (Equals and GetHashCode) ensure that this class can be compared using IPAddress and Port only ignoring everything else.
        public override bool Equals(object obj)
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
