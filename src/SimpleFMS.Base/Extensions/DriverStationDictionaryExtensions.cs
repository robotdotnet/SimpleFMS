using System.Collections.Generic;
using SimpleFMS.Base.DriverStation;

namespace SimpleFMS.Base.Extensions
{
    public static class DriverStationDictionaryExtensions
    {
        public static bool IsReadyToStartMatch(this IReadOnlyDictionary<AllianceStation, IDriverStationReport> report)
        {
            bool hasNotReadyDs = false;
            foreach (IDriverStationReport driverStation in report.Values)
            {
                if (driverStation.IsBypassed)
                    continue;
                if (driverStation.DriverStationConnected && driverStation.RoboRioConnected)
                    continue;
                hasNotReadyDs = true;
            }
            return !hasNotReadyDs;
        }
    }
}
