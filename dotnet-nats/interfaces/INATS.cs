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
        void Publish(string subject, string data, Action<string> handler = null);        
        void Subscribe(string subject, Action<string> handler);
        void Unsubscribe(string subject);
        void Flush();
    }
}
