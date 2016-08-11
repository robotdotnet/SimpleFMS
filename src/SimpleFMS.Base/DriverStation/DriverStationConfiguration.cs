using System;

namespace SimpleFMS.Base.DriverStation
{
    /// <summary>
    /// A class used to set configuration data for driver stations
    /// </summary>
    public struct DriverStationConfiguration : IDriverStationConfiguration, IEquatable<DriverStationConfiguration>
    {
        /// <summary>
        /// Creates a new <see cref="DriverStationConfiguration"/>
        /// </summary>
        /// <param name="teamNumber">The team number</param>
        /// <param name="station">The alliance station</param>
        /// <param name="isBypassed">True if the station should be bypassed</param>
        public DriverStationConfiguration(int teamNumber, AllianceStation station, bool isBypassed)
        {
            TeamNumber = teamNumber;
            Station = station;
            IsBypassed = isBypassed;
        }

        /// <inheritdoc/>
        public bool Equals(DriverStationConfiguration other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TeamNumber == other.TeamNumber && Station.Equals(other.Station) && IsBypassed == other.IsBypassed;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DriverStationConfiguration) obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = TeamNumber;
                hashCode = (hashCode*397) ^ Station.GetHashCode();
                hashCode = (hashCode*397) ^ IsBypassed.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Gets if these 2 instances are equal
        /// </summary>
        /// <param name="left">The left operator</param>
        /// <param name="right">The right operator</param>
        /// <returns>True if the values are equal</returns>
        public static bool operator ==(DriverStationConfiguration left, DriverStationConfiguration right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Gets if these 2 instances are not equal
        /// </summary>
        /// <param name="left">The left operator</param>
        /// <param name="right">The right operator</param>
        /// <returns>True if the values are not equal</returns>
        public static bool operator !=(DriverStationConfiguration left, DriverStationConfiguration right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public int TeamNumber { get; }
        /// <inheritdoc/>
        public AllianceStation Station { get; }
        /// <inheritdoc/>
        public bool IsBypassed { get; }
    }
}
