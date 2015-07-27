using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_sockets;

namespace dotnet_nats
{
    public class Server : IServer
    {
        public Server() { ReconnectAttempts = 0; }
        public string URL { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool Connected { get { return this.Transport.IsConnected; } }
        public int ReconnectAttempts { get; set; }
        public ITransport Transport { get; set; }
    }
}
