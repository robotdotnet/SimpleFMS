using System;
using System.Collections.Generic;
using System.Threading;
using NetworkTables;
using NetworkTables.Independent;
using SimpleFMS.Base.DriverStation;
using SimpleFMS.Base.MatchTiming;
using SimpleFMS.Base.Networking;
using SimpleFMS.Networking.Base;
using SimpleFMS.Networking.Server.NetworkTableUpdaters;
using static SimpleFMS.Base.Networking.NetworkingConstants;

namespace SimpleFMS.Networking.Server
{
    public class NetworkServerManager : INetworkServerManager
    {
        private readonly List<NetworkTableUpdaterBase> m_networkTableUpdaters = new List<NetworkTableUpdaterBase>();

        private readonly object m_lockObject = new object();

        private const int TableUpdatePeriod = 102;

        private readonly IndependentNetworkTable m_networkTableRoot;
        private readonly IndependentNtCore m_ntCore;
        private readonly IndependentRemoteProcedureCall m_rpc;

        private readonly Timer m_updateTimer;

        public NetworkServerManager(IDriverStationManager driverStationManager, IMatchTimingManager matchTimingManager)
        {
            m_ntCore = new IndependentNtCore();
            m_ntCore.UpdateRate = 0.5;
            m_ntCore.RemoteName = FmsServerRemoteName;
            m_ntCore.StartServer(PersistentFilename, "", NetworkTablesPort);
            m_rpc = new IndependentRemoteProcedureCall(m_ntCore);
            m_networkTableRoot = new IndependentNetworkTable(m_ntCore, RootTableName);

            m_networkTableUpdaters.Add(new DriverStationUpdater(m_networkTableRoot,m_rpc, driverStationManager));
            m_networkTableUpdaters.Add(new MatchTimingUpdater(m_networkTableRoot, m_rpc, matchTimingManager));

            m_updateTimer = new Timer(OnTimerUpdate, null, TableUpdatePeriod, TableUpdatePeriod);

            m_networkTableRoot.AddConnectionListener((remote, info, conn) =>
            {
                OnClientChanged?.Invoke(info.RemoteId, info.RemoteIp, conn);
            }, true);
        }

        public void Dispose()
        {
            m_updateTimer.Dispose();

            lock (m_lockObject)
            {
                foreach (var updater in m_networkTableUpdaters)
                {
                    updater.Dispose();
                }
            }

            m_ntCore.Dispose();
        }

        private void OnTimerUpdate(object state)
        {
            lock (m_lockObject)
            {
                foreach (var updater in m_networkTableUpdaters)
                {
                    updater.UpdateTable();
                }
            }
            NetworkTable.Flush();
        }

        public event Action<string, string, bool> OnClientChanged;
    }
}
