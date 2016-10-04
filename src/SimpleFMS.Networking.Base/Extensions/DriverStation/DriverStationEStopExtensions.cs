using SimpleFMS.Base.DriverStation;
using System.Collections.Generic;

namespace SimpleFMS.Networking.Base.Extensions.DriverStation
{
    public static class DriverStationEStopExtensions
    {
        public static IList<byte> PackDriverStationSetEStop(this AllianceStation station, bool eStop)
        {
            IList<byte> data = new byte[3];
            data[0] = (byte)CustomNetworkTableType.DriverStationUpdateEStop;
            data[1] = station.GetByte();
            data[2] = (byte)(eStop ? 1 : 0);
            return data;
        }

        public static AllianceStation GetDriverStationToEStop(this IList<byte> value, out bool eStop, out bool isValid)
        {
            eStop = false;
            isValid = false;

            if (value.Count < 3)
               return new AllianceStation();

            if (value[0] != (byte)CustomNetworkTableType.DriverStationUpdateEStop) return new AllianceStation();
            var station = new AllianceStation(value[1]);
            eStop = value[2] != 0;
            isValid = true;
            return station;
        }

        public static IList<byte> PackDriverStationUpdateEStopResponse(bool set)
        {
            return new[] { (byte)CustomNetworkTableType.DriverStationUpdateEStop, (byte)(set ? 1 : 0) };
        }

        public static bool UnpackDriverStationUpdateEStopResponse(this IList<byte> value)
        {
            if (value.Count < 2)
                return false;
            if (value[0] != (byte)CustomNetworkTableType.DriverStationUpdateEStop)
                return false;
            return value[1] != 0;
        }


        public static IList<byte> PackDriverStationGlobalEStop()
        {
            IList<byte> data = new byte[1];
            data[0] = (byte)CustomNetworkTableType.DriverStationGlobalEStop;
            return data;
        }

        public static bool GetGlobalDriverStationEStop(this IList<byte> value)
        { 
            if (value.Count < 1)
                return false;

            if (value[0] != (byte)CustomNetworkTableType.DriverStationGlobalEStop) return false;
            return true;
        }

        public static IList<byte> PackDriverStationGlobalEStopResponse()
        {
            return new[] { (byte)CustomNetworkTableType.DriverStationGlobalEStop};
        }

        public static bool UnpackDriverStationGlobalEStopResponse(this IList<byte> value)
        {
            if (value.Count < 1)
                return false;
            if (value[0] != (byte)CustomNetworkTableType.DriverStationGlobalEStop)
                return false;
            return true;
        }
    }
}
