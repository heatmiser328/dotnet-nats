using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface IFactory
    {
        IServer NewServer(string url);
        ICollection<IServer> NewServer(string[] urls);

        ITransport NewTransport(string address, int port);

        IMessenger NewMessenger();
    }
}
