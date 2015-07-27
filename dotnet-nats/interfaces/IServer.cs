using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface IServer
    {
        string URL { get; }
        string Address { get; }
        int Port { get; }
        bool Connected { get; }
        int ReconnectAttempts { get; set; }
        ITransport Transport { get; }
    }
}
