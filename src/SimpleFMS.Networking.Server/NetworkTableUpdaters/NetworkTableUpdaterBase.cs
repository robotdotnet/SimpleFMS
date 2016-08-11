using System;
using System.Collections.Generic;
using NetworkTables;
using NetworkTables.Tables;

namespace SimpleFMS.Networking.Server.NetworkTableUpdaters
{
    internal abstract class NetworkTableUpdaterBase : IDisposable
    {
        protected readonly ITable NetworkTable;

        protected List<Action<ITable, string, Value, NotifyFlags>> NetworkTableListeners { get; } =
            new List<Action<ITable, string, Value, NotifyFlags>>();

        protected NetworkTableUpdaterBase(ITable root, string tableName)
        {
            if (root == null) 
                throw new ArgumentNullException(nameof(root), "Table root cannot be null");

            NetworkTable = root.GetSubTable(tableName);
        }

        public abstract void UpdateTable();

        protected void AddTableListener(string key, Action<ITable, string, Value, NotifyFlags> listener, 
            NotifyFlags flags = NotifyFlags.NotifyImmediate)
        {
            NetworkTableListeners.Add(listener);
            NetworkTable.AddTableListenerEx(key, listener, flags);
        }

        public virtual void Dispose()
        {
            foreach (var networkTableListener in NetworkTableListeners)
            {
                NetworkTable.RemoveTableListener(networkTableListener);
            }
        }
    }
}
