using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Shouldly;
using NSubstitute;

using dotnet_nats;

#pragma warning disable 4014

namespace tests
{
    public class SubscribeTests
    {
        static int _sid = 1;
        ILog _log;
        IFactory _factory;
        IServer _server;        
        IMessenger _msgr;
        ICollection<IServer> _servers;
        Options _opts;
        const string cURL = "nats://domain:4222";
        const string cConnect = @"CONNECT {""verbose"":false,""pedantic"":false}" + "\r\n";
        const string PONG = "PONG\r\n";

        public SubscribeTests()
        {            
            _opts = new Options();            
            _opts.uris = new List<string>() {cURL};
            _servers = new List<IServer>();
            _log = Substitute.For<ILog>();

            _msgr = Substitute.For<IMessenger>();
                    
            _server = Substitute.For<IServer>();
            _server.IsConnected.Returns(true);
            _server.URL.Returns(cURL);
            _server.Open().Returns(Task<bool>.FromResult(true));
            _server.Send(Arg.Any<string>())
                .Returns(Task<int>.FromResult(1))
                .AndDoes(x => { _server.Sent += Raise.EventWith(new dotnet_sockets.EventArgs<int>(1)); });            
            _servers.Add(_server);
            _factory = Substitute.For<IFactory>();
            _factory.NewServer(Arg.Any<string[]>()).Returns(_servers);
            _factory.NewMessenger().Returns(_msgr);
        }

        [Fact]
        public async Task Subscribe()
        {            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _server.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            await nats.Connect();
            nats.Subscribe("a", Substitute.For<Action<string>>());
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
        }

        [Fact]
        public async Task SubscribeMultiple()
        {            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _server.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            await nats.Connect();
            nats.Subscribe("a", Substitute.For<Action<string>>());
            nats.Subscribe("b", Substitute.For<Action<string>>());
            nats.Subscribe("a", Substitute.For<Action<string>>());
            _server.Received(2).Send(Arg.Any<string>());
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("b")));
        }

        //[Fact]
        public async Task SubscribeThenConnect()
        {            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _server.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            nats.Subscribe("a", Substitute.For<Action<string>>());
            _server.Received(0).Send(Arg.Any<string>());

            await nats.Connect();
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
        }

        [Fact]
        public void Unsubscribe()
        {            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _server.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            nats.Subscribe("a", Substitute.For<Action<string>>());
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
            var sid = _sid - 1;

            nats.Unsubscribe("a");
            _server.Received(1).Send(Arg.Is<string>(makeUnsubscription(sid)));
        }

        [Fact]
        public void UnsubscribeMultiple()
        {            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _server.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            nats.Subscribe("a", Substitute.For<Action<string>>());            
            nats.Subscribe("b", Substitute.For<Action<string>>());
            
            var asid = _sid;
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
            var bsid = _sid;
            _server.Received(1).Send(Arg.Is<string>(makeSubscription("b")));
            
            nats.Unsubscribe("a");
            nats.Unsubscribe("b");

            _server.Received(4).Send(Arg.Any<string>());
            _server.Received(1).Send(Arg.Is<string>(makeUnsubscription(asid)));
            _server.Received(1).Send(Arg.Is<string>(makeUnsubscription(bsid)));
        }

        string makeSubscription(string subject, string queue = " ")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SUB {0} {1} {2}{3}", subject, queue, _sid++, System.Environment.NewLine);
            return sb.ToString();
        }

        string makeUnsubscription(int sid)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("UNSUB {0} 0{1}", sid, System.Environment.NewLine);
            return sb.ToString();
        }

    }
}
