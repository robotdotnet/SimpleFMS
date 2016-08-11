using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SimpleFMS.DriverStation.UdpControllers
{
    internal static class NetStandardUdpExtensions
    {
#if NETSTANDARD
        public static int Send(this UdpClient client, byte[] dgram, int bytes, EndPoint ipEp)
        {
            return client.Client.SendTo(dgram, bytes, SocketFlags.None, ipEp);
        }
#endif
    }
}
