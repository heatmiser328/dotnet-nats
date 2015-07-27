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
            ITransport t = new TcpTransport(address, port);
            t.Log += (sender, args) =>
            {
                _log.Log(args.Level, args.Message, args.Exception, args.Args);
            };
            return t;
        }
    }
}
