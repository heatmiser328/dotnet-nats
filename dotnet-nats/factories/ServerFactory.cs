using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class ServerFactory : IServerFactory
    {
        ITransportFactory _factory;
        ILog _log;

        public ServerFactory(ITransportFactory factory, ILog log)
        {
            _factory = factory;
            _log = log;
        }

        public IServer New(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                UriBuilder uri = new UriBuilder(url);
                return new Server {
                    URL = url,
                    Address = uri.Host,
                    Port = uri.Port,
                    Transport = _factory.New(uri.Host, uri.Port)                    
                };
            }
            return null;
        }

        public ICollection<IServer> New(string[] urls)
        {
            ICollection<IServer> servers = new List<IServer>();
            foreach (string url in urls)
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    _log.Warn("Unrecognized server url: {0}. Skipping...", url);
                    continue;
                }
                servers.Add(New(url));
            }
            return servers;
        }
    }
}
