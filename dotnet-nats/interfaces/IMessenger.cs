using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface IMessenger
    {
        void Ping(Action<string> handler);
        void Receive(byte[] data, int size);
    }
}
