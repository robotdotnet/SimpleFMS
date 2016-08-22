using System;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using NetworkTables.Tables;
using NetworkTables.Independent;
using SimpleFMS.Base.MatchTiming;
using SimpleFMS.Networking.Base;
using SimpleFMS.Networking.Base.Extensions.MatchTiming;
using static SimpleFMS.Networking.Base.NetworkingConstants.MatchTimingConstants;
using static SimpleFMS.Networking.Base.Extensions.MatchTiming.MatchTimingMatchStateExtensions;

namespace SimpleFMS.Networking.Client.NetworkClients
{
    public class MatchTimingClient : NetworkClientBase
    {
        private readonly IndependentRemoteProcedureCall m_rpc;

        public MatchTimingClient(INetworkClientManager manager) 
            : base(manager.NetworkTable, MatchTimingTableName)
        {
            m_rpc = manager.Rpc;

            var flags = NotifyFlags.NotifyImmediate | NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            AddTableListener(MatchStatusReportKey, OnMatchTimeReportCallback, flags); 
        }

        public event Action<IMatchStateReport> OnMatchTimeReportChanged;

        private void OnMatchTimeReportCallback(ITable table, string key, Value value, NotifyFlags flags)
        {
            if (OnMatchTimeReportChanged == null) return;
            if (!value.IsRaw()) return;
            if (!key.Contains(MatchStatusReportKey)) return;
            var data = value.GetRaw().GetMatchTimingReport();
            if (data == null) return;
            OnMatchTimeReportChanged?.Invoke(data);
        }

        public IMatchStateReport GetMatchStateReport()
        {
            var value = NetworkTable.GetRaw(MatchStatusReportKey, null);
            return value?.GetMatchTimingReport();
        }

        public async Task<bool> StartMatch(CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(StartMatchRpcKey, token, PackStartMatch());
            if (data == null) return false;
            return data.UnpackStartMatchResponse();
        }

        public async Task<bool> StopPeriod(CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(StopPeriodRpcKey, token, PackStopPeriod());
            if (data == null) return false;
            return data.UnpackStopPeriodResponse();
        }

        public async Task<bool> StartAutonomous(CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(StartAutonomousRpcKey, token, PackStartAutonomous());
            if (data == null) return false;
            return data.UnpackStartAutonomousResponse();
        }

        public async Task<bool> StartTeleoperated(CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(StartTeleoperatedRpcKey, token, PackStartTeleoperated());
            if (data == null) return false;
            return data.UnpackStartTeleoperatedResponse();
        }

        public async Task<bool> SetMatchPeriodTimes(IMatchTimeReport times, CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(SetMatchPeriodTimeRpcKey, token, times.PackMatchTimes());
            if (data == null) return false;
            return data.UnpackMatchTimesResponse();
        }
    }
}
