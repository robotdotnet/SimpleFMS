using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

            var task = Dns.GetHostAddressesAsync(Dns.GetHostName());
            task.Wait();
            if (!task.IsCompleted)
            {
                Environment.FailFast("Unknown error while trying to parse local IP addresses");
            }

            bool foundIP = false;

            foreach (IPAddress ip in task.Result)
            {
                if (ip.Equals(IPAddress.Parse("10.0.100.5")))
                {
                    foundIP = true;
                    break;
                }
            }
            if (!foundIP)
            {
                Console.WriteLine("Ip Address must be set to 10.0.100.5 in order to work properly");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Proper IP Address found");

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
