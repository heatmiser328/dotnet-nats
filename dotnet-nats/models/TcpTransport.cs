using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class TcpTransport : dotnet_sockets.AsyncSocketClient, ITransport
    {
        public TcpTransport() : base() {}
        public TcpTransport(string address, int port, ILog log) : base(address, port, log) {}
    }
}
