using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface INATS
    {
        int Servers { get; }
        bool Connected { get; }

        Task<bool> Connect(Action<bool> handler = null);
        void Close();
        void Subscribe(string topic, Action<string> handler);
        void Unsubscribe(string topic);
        void Publish(string topic, string data);
        void Publish(string topic, byte[] data);
        void Flush();
    }
}
