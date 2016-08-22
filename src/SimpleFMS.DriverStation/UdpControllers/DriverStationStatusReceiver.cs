using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SimpleFMS.DriverStation.Enums;
using SimpleFMS.DriverStation.UdpData;

namespace SimpleFMS.DriverStation.UdpControllers
{
    public class DriverStationStatusReceiver
    {
        private UdpClient m_client = null;
        private IPEndPoint m_endPoint;
        private readonly object m_lockObject = new object();

        private Task m_readTask;
        private CancellationTokenSource m_tokenSource;

        public event Action<DriverStationStatusData> OnDriverStationReceive;

        public DriverStationStatusReceiver(int port)
        {
            m_endPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public void Restart()
        {
            m_tokenSource?.Cancel();
            m_readTask?.Wait();

            ((IDisposable)m_client)?.Dispose();

            m_client = new UdpClient(m_endPoint);

            m_tokenSource = new CancellationTokenSource();

            m_readTask = Task.Run(() =>
            {
                while (!m_tokenSource.IsCancellationRequested)
                {
                    try
                    {
                        Task<UdpReceiveResult> task = m_client.ReceiveAsync();
                        task.Wait();
                        if (task.IsCompleted)
                        {
                            lock (m_lockObject)
                            {
                                if (m_client == null) return;
                                try
                                {
                                    DriverStationStatusData parsedData = new DriverStationStatusData();
                                    var result = parsedData.ParseData(task.Result.Buffer);
                                    if (result == ReceiveParseStatus.TrackedPacketValid)
                                        OnDriverStationReceive?.Invoke(parsedData);
                                }
                                catch (ObjectDisposedException)
                                {
                                    // Ignore an Object Disposed Exception
                                }
                            }
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Do nothing, return
                        return;
                    }
                }

            }, m_tokenSource.Token);

            m_readTask = m_client.ReceiveAsync();
        }

        public void Dispose()
        {
            OnDriverStationReceive = null;
            ((IDisposable)m_client)?.Dispose();
            m_client = null;
        }
    }
}
