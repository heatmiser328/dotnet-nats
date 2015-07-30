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
    public class FactoryTest
    {        
        ILog _log;                
        const string cURL = "nats://domain:4222";

        public FactoryTest()
        {
            _log = Substitute.For<ILog>();                        
        }

        [Fact]
        public void Instantiate()
        {
            IFactory factory = new Factory(_log);
            factory.ShouldNotBe(null);
        }

        [Fact]
        public void New_Single()
        {
            IFactory factory = Substitute.ForPartsOf<Factory>(_log);            

            IServer server = factory.NewServer(cURL);
            server.ShouldNotBe(null);
            server.IsConnected.ShouldBe(false);
            server.URL.ShouldBe(cURL);
            server.Address.ShouldBe("domain");
            server.Port.ShouldBe(4222);
        }

        [Fact]
        public void New_Multiple()
        {
            const string cURL2 = "nats://domain2:8888";
            const string cURL3 = "nats://domain3:1234";

            IFactory factory = Substitute.ForPartsOf<Factory>(_log);            

            ICollection<IServer> servers = factory.NewServer(new string[] {cURL,cURL2,cURL3});
            servers.ShouldNotBe(null);
            servers.Count.ShouldBe(3);
            IEnumerator<IServer> current = servers.GetEnumerator();
            current.MoveNext();            
            IServer server = current.Current;
            server.IsConnected.ShouldBe(false);
            server.URL.ShouldBe(cURL);
            server.Address.ShouldBe("domain");
            server.Port.ShouldBe(4222);

            current.MoveNext();
            server = current.Current;
            server.IsConnected.ShouldBe(false);
            server.URL.ShouldBe(cURL2);
            server.Address.ShouldBe("domain2");
            server.Port.ShouldBe(8888);

            current.MoveNext();
            server = current.Current;
            server.IsConnected.ShouldBe(false);
            server.URL.ShouldBe(cURL3);
            server.Address.ShouldBe("domain3");
            server.Port.ShouldBe(1234);
        }

    }
}
