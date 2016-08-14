using System;

namespace SimpleFMS.Networking.Base
{
    public interface INetworkServerManager : IDisposable
    {
        event Action<string, string, bool> OnClientChanged;
    }
}
