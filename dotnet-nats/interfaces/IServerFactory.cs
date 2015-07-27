using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface IServerFactory
    {        
        IServer New(string url);
        ICollection<IServer> New(string[] urls);
    }
}
