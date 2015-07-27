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
    public class ServerFactoryTest
    {        
        ILog _log;
        ITransportFactory _factory;
        ITransport _transport;
        const string cURL = "nats://domain:4222";

        public ServerFactoryTest()
        {
            _log = Substitute.For<ILog>();            
            _transport = Substitute.For<ITransport>();
            _factory = Substitute.For<ITransportFactory>();
            _factory.New(Arg.Any<string>(), Arg.Any<int>()).Returns(_transport);
        }

        [Fact]
        public void Instantiate()
        {
            IServerFactory factory = new ServerFactory(_factory, _log);
            factory.ShouldNotBe(null);
        }

        [Fact]
        public void New_Single()
        {
            IServerFactory factory = new ServerFactory(_factory, _log);
            factory.ShouldNotBe(null);
            IServer server = factory.New(cURL);
            server.ShouldNotBe(null);
            server.Connected.ShouldBe(false);
            server.URL.ShouldBe(cURL);
            server.Address.ShouldBe("domain");
            server.Port.ShouldBe(4222);
        }

        [Fact]
        public void New_Multiple()
        {
            const string cURL2 = "nats://domain2:8888";
            const string cURL3 = "nats://domain3:1234";

            IServerFactory factory = new ServerFactory(_factory, _log);
            factory.ShouldNotBe(null);
            ICollection<IServer> servers = factory.New(new string[] {cURL,cURL2,cURL3});
            servers.ShouldNotBe(null);
            servers.Count.ShouldBe(3);
            IEnumerator<IServer> current = servers.GetEnumerator();
            current.MoveNext();            
            IServer server = current.Current;
            server.Connected.ShouldBe(false);
            server.URL.ShouldBe(cURL);
            server.Address.ShouldBe("domain");
            server.Port.ShouldBe(4222);

            current.MoveNext();
            server = current.Current;            
            server.Connected.ShouldBe(false);
            server.URL.ShouldBe(cURL2);
            server.Address.ShouldBe("domain2");
            server.Port.ShouldBe(8888);

            current.MoveNext();
            server = current.Current;
            server.Connected.ShouldBe(false);
            server.URL.ShouldBe(cURL3);
            server.Address.ShouldBe("domain3");
            server.Port.ShouldBe(1234);
        }

    }
}
