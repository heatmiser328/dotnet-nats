using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class Factory : IFactory
    {
        ILog _log;

        public Factory(ILog log)
        {            
            _log = log;
        }

        public IServer NewServer(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                UriBuilder uri = new UriBuilder(url);
                return new Server
                {
                    URL = url,
                    Address = uri.Host,
                    Port = uri.Port,
                    Transport = NewTransport(uri.Host, uri.Port)
                };
            }
            return null;            
        }

        public ICollection<IServer> NewServer(string[] urls)
        {
            ICollection<IServer> servers = new List<IServer>();
            foreach (string url in urls)
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    _log.Warn("Unrecognized server url: {0}. Skipping...", url);
                    continue;
                }
                servers.Add(NewServer(url));
            }
            return servers;            
        }

        public virtual ITransport NewTransport(string address, int port)
        {
            ITransport t = new TcpTransport(address, port);
            t.Log += (sender, args) =>
            {
                _log.Log(args.Level, args.Message, args.Exception, args.Args);
            };
            return t;
        }

        public IMessenger NewMessenger()
        {
            return new Messenger(_log);
        }
    }
}
