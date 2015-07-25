using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_sockets;

namespace dotnet_nats
{
    class Server    
    {
        public Server(string url, ILog log)
        {
            URL = url;
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                UriBuilder uri = new UriBuilder(url);
                Address = uri.Host;
                Port = uri.Port;
                Transport = new AsyncSocketClient(uri.Host, uri.Port, log);
            }
        }

        public string URL { get; private set; }
        public string Address { get; private set; }
        public int Port { get; private set; }
        public ISocketClient Transport { get; private set; }

    }
}
