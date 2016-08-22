using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NetworkTables;
using NetworkTables.Independent;
using NetworkTables.Tables;
using SimpleFMS.Base.DriverStation;
using SimpleFMS.Base.Enums;
using SimpleFMS.Networking.Base;
using SimpleFMS.Networking.Base.Extensions.DriverStation;
using SimpleFMS.Base.Extensions;
using static SimpleFMS.Networking.Base.NetworkingConstants.DriverStationConstants;

namespace SimpleFMS.Networking.Client.NetworkClients
{
    public class DriverStationClient : NetworkClientBase
    {
        private readonly IndependentRemoteProcedureCall m_rpc;

        public DriverStationClient(INetworkClientManager manager) 
            : base(manager.NetworkTable, DriverStationTableName)
        {
            m_rpc = manager.Rpc;

            var flags = NotifyFlags.NotifyImmediate | NotifyFlags.NotifyNew | NotifyFlags.NotifyUpdate;
            AddTableListener(DriverStationReportKey, OnDriverStationReportCallback, flags);
        }

        public event Action<IReadOnlyDictionary<AllianceStation, IDriverStationReport>> OnDriverStationReportsChanged;

        public void OnDriverStationReportCallback(ITable table, string key, Value value, NotifyFlags flags)
        {
            if (OnDriverStationReportsChanged == null) return;
            if (!value.IsRaw()) return;
            if (!key.Contains(DriverStationReportKey)) return;
            var data = value.GetRaw().GetDriverStationReports();
            if (data == null) return;
            OnDriverStationReportsChanged?.Invoke(data);
        }

        public IReadOnlyDictionary<AllianceStation, IDriverStationReport> GetDriverStationReports()
        {
            var value = NetworkTable.GetRaw(DriverStationReportKey, null);
            return value?.GetDriverStationReports();
        }

        public bool RequiresAllRobotsConnectedOrBypassed()
        {
            return NetworkTable.GetBoolean(DriverStationRequiresAllConnectedOrBypassed, true);
        }

        public bool IsReadyToStartMatch()
        {
            if (!RequiresAllRobotsConnectedOrBypassed()) return true;
            var reports = GetDriverStationReports();
            if (reports == null) return false;
            return reports.IsReadyToStartMatch();
        }

        public override void Dispose()
        {
            OnDriverStationReportsChanged = null;
            base.Dispose();
        }

        public async Task<bool> UpdateDriverStationEStop(AllianceStation station, bool eStopped, CancellationToken token)
        {
            byte[] data =
                await
                    m_rpc.CallRpcWithResultAsync(DriverStationUpdateBypassRpcKey, token,
                        station.PackDriverStationSetEStop(eStopped));
            if (data == null) return false;
            return data.UnpackDriverStationUpdateEStopResponse();
        }

        public async Task<bool> UpdateDriverStationBypass(AllianceStation station, bool bypass, CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(DriverStationUpdateBypassRpcKey, token,
                station.PackDriverStationSetBypass(bypass));
            if (data == null) return false;
            return data.UnpackDriverStationUpdateBypassResponse();
        }

        public async Task<bool> UpdateDriverStationConfigurations(
            IReadOnlyList<IDriverStationConfiguration> configuration, int matchNumber, MatchType matchType, 
            CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(DriverStationSetConfigurationRpcKey,token,
                configuration.PackDriverStationConfigurationData(matchNumber, matchType));
            if (data == null) return false;
            return data.UnpackDriverStationSetConfigurationResponse();
        }

        public async Task<bool> GlobalEStop(CancellationToken token)
        {
            byte[] data = await m_rpc.CallRpcWithResultAsync(DriverStationGlobalEStopRpcKey, token,
                DriverStationEStopExtensions.PackDriverStationGlobalEStop());
            if (data == null) return false;
            return data.UnpackDriverStationGlobalEStopResponse();
        }
    }
}
