using System;
using System.Net;
using System.Net.Sockets;
using SimpleFMS.DriverStation.UdpData;

namespace SimpleFMS.DriverStation.UdpControllers
{
    public class DriverStationControlSender
    {
        private UdpClient m_client;

        public DriverStationControlSender()
        {
            m_client = new UdpClient();
        }

        public void SendPacket(IPEndPoint ipEp, DriverStationControlData data)
        {
            try
            {
                byte[] packet = data.PackData();
                // TODO: Send async back to the manager.
                m_client.SendAsync(packet, packet.Length, ipEp).Wait();
            }
            catch (SocketException)
            {
                // TODO: Log
            }
            catch (ObjectDisposedException)
            {
                // Ignore Disposed Exception
            }
        }

        public void Restart()
        {
            Dispose();
            m_client = new UdpClient();
        }

        public void Dispose()
        {
            ((IDisposable)m_client)?.Dispose();
            m_client = null;
        }
    }
}
