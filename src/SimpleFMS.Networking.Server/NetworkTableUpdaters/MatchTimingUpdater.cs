using System.Collections.Generic;
using NetworkTables;
using NetworkTables.Independent;
using NetworkTables.Tables;
using SimpleFMS.Base.MatchTiming;
using SimpleFMS.Networking.Base.Extensions.MatchTiming;
using static SimpleFMS.Networking.Base.NetworkingConstants.MatchTimingConstants;
using static SimpleFMS.Networking.Base.Extensions.MatchTiming.MatchTimingMatchStateExtensions;

namespace SimpleFMS.Networking.Server.NetworkTableUpdaters
{
    internal class MatchTimingUpdater : NetworkTableUpdaterBase
    {
        private readonly IMatchTimingManager m_matchTimingManager;
        private readonly IndependentRemoteProcedureCall m_rpc;

        public MatchTimingUpdater(ITable root, IndependentRemoteProcedureCall rpc, IMatchTimingManager timingManager) 
            : base(root, MatchTimingTableName)
        {
            m_rpc = rpc;
            m_matchTimingManager = timingManager;

            m_rpc.CreateRpc(StartMatchRpcKey, new RpcDefinition(StartMatchRpcVersion, StartMatchRpcKey),
                StartMatchCallback);

            m_rpc.CreateRpc(StopPeriodRpcKey, new RpcDefinition(StopPeriodRpcVersion, StopPeriodRpcKey),
                StopPeriodCallback);

            m_rpc.CreateRpc(StartAutonomousRpcKey, new RpcDefinition(StartAutonomousRpcVersion, StartAutonomousRpcKey),
                StartAutonomousCallback);

            m_rpc.CreateRpc(StartTeleoperatedRpcKey,
                new RpcDefinition(StartTeleoperatedRpcVersion, StartTeleoperatedRpcKey), StartTeleoperatedCallback);

            m_rpc.CreateRpc(SetMatchPeriodTimeRpcKey,
                new RpcDefinition(SetMatchPeriodTimeRpcVersion, SetMatchPeriodTimeRpcKey), SetMatchPeriodTimeCallback);
        }

        private IList<byte> StartMatchCallback(string name, IList<byte> bytes, ConnectionInfo connInfo)
        {
            var good = bytes.UnpackStartMatch();
            bool started = false;
            if (good)
                started = m_matchTimingManager.StartMatch();
            return PackStartMatchResponse(started);
        }

        private IList<byte> StopPeriodCallback(string name, IList<byte> bytes, ConnectionInfo connInfo)
        {
            var good = bytes.UnpackStopPeriod();
            if (good) m_matchTimingManager.StopCurrentPeriod();
            return PackStopPeriodResponse(good);
        }

        private IList<byte> StartAutonomousCallback(string name, IList<byte> bytes, ConnectionInfo connInfo)
        {
            var good = bytes.UnpackStartAutonomous();
            bool started = false;
            if (good)
                started = m_matchTimingManager.StartAutonomous();
            return PackStartAutonomousResponse(started);
        }

        private IList<byte> StartTeleoperatedCallback(string name, IList<byte> bytes, ConnectionInfo connInfo)
        {
            var good = bytes.UnpackStartTeleoperated();
            bool started = false;
            if (good)
                started = m_matchTimingManager.StartTeleop();
            return PackStartTeleoperatedResponse(started);
        }

        private IList<byte> SetMatchPeriodTimeCallback(string name, IList<byte> bytes, ConnectionInfo connInfo)
        {
            var data = bytes.UnpackMatchTimes();
            bool success = false;
            if (data != null)
            {
                success = m_matchTimingManager.SetMatchTimes(data);
            }
            return PackMatchTimesResponse(success);
        }

        public override void UpdateTable()
        {
            var report = m_matchTimingManager.GetMatchTimingReport();

            NetworkTable.PutRaw(MatchStatusReportKey, report.PackMatchTimingReport());
        }
    }
}
