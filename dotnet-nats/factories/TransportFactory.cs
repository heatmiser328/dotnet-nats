using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class TransportFactory : ITransportFactory
    {
        ILog _log;
        public TransportFactory(ILog log)
        {
            _log = log;
        }

        public ITransport New(string address, int port)
        {
            return new TcpTransport(address, port, _log);
        }
    }
}
