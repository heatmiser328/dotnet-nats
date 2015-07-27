using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Shouldly;
using NSubstitute;

using dotnet_nats;

namespace tests
{
    public class InstantiateTests
    {
        ILog _log;
        IServerFactory _factory;
        IServer _server;
        ITransport _transport;
        ICollection<IServer> _servers;
        const string cURL = "nats://domain:4222";
        
        public InstantiateTests()
        {
            _servers = new List<IServer>();
            _log = Substitute.For<ILog>();
            _transport = Substitute.For<ITransport>();
            _server = Substitute.For<IServer>();
            _server.Transport.Returns(_transport);
            _servers.Add(_server);
            _factory = Substitute.For<IServerFactory>();            
            _factory.New(Arg.Any<string[]>()).Returns(_servers);
        }

        [Fact]
        public void Instantiate_SingleServer()
        {
            IDictionary<string, object> opts = new Dictionary<string, object>();
            opts.Add("url", cURL);

            INATS nats = new NATS(_factory, opts, _log);            
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
        }

        [Fact]
        public void Instantiate_MultipleServer()
        {
            IDictionary<string, object> opts = new Dictionary<string, object>();
            opts.Add("urls", new string[] {cURL,cURL,cURL});
            _servers.Add(_server);
            _servers.Add(_server);

            INATS nats = new NATS(_factory, opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(3);
            nats.Connected.ShouldBe(false);
        }

    }
}
