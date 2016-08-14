using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleFMS.DriverStation;
using SimpleFMS.MatchTiming;
using SimpleFMS.Networking.Server;
using SimpleFMS.Base.DriverStation;
using SimpleFMS.Base.MatchTiming;
using SimpleFMS.Networking.Base;

namespace SimpleFMS.ConsoleHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Simple FMS");

            IDriverStationManager dsManager = new DriverStationManager();
            IMatchTimingManager matchManager = new MatchTimingManager(dsManager);
            INetworkServerManager networkManager = new NetworkServerManager(dsManager, matchManager);

            networkManager.OnClientChanged += (id, ip, conn) =>
            {
                if (conn)
                {
                    Console.WriteLine($"Client connected: {id} at {ip}");
                }
                else
                {
                    Console.WriteLine($"Client disconnected: {id} at {ip}");
                }
            };

            Thread.Sleep(Timeout.Infinite);

            GC.KeepAlive(networkManager);
        }
    }
}
