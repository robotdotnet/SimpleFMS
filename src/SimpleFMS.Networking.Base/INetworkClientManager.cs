using System;
using NetworkTables;
using NetworkTables.Independent;

namespace SimpleFMS.Networking.Base
{
    public interface INetworkClientManager : IDisposable
    {
        bool FmsConnected { get; }

        void AddClient(INetworkClient client);

        void SuspendNetworking();
        void ResumeNetworking();

        event Action<bool> OnFmsConnectionChanged;

        IndependentNtCore NtCore { get; }
        IndependentNetworkTable NetworkTable { get; }
        IndependentRemoteProcedureCall Rpc { get; }
    }
}
