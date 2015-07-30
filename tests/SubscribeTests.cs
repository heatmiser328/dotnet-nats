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
        int _sid;
        ILog _log;
        IFactory _factory;
        IServer _server;
        ITransport _transport;
        IMessenger _msgr;
        ICollection<IServer> _servers;
        Options _opts;
        const string cURL = "nats://domain:4222";
        const string cConnect = @"CONNECT {""verbose"":false,""pedantic"":false}" + "\r\n";
        const string PONG = "PONG\r\n";

        public SubscribeTests()
        {
            _sid = 1;
            _opts = new Options();            
            _opts.uris = new List<string>() {cURL};
            _servers = new List<IServer>();
            _log = Substitute.For<ILog>();

            _msgr = Substitute.For<IMessenger>();

            _transport = Substitute.For<ITransport>();
            _transport.Open().Returns(Task<bool>.FromResult(true));
            _transport.Send(Arg.Any<string>())
                .Returns(Task<int>.FromResult(1))
                .AndDoes(x => { _transport.Sent += Raise.EventWith(new dotnet_sockets.EventArgs<int>(1));});
                    
            _server = Substitute.For<IServer>();
            _server.Connected.Returns(true);
            _server.URL.Returns(cURL);
            _server.Transport.Returns(_transport);
            _servers.Add(_server);
            _factory = Substitute.For<IFactory>();
            _factory.NewServer(Arg.Any<string[]>()).Returns(_servers);
            _factory.NewMessenger().Returns(_msgr);
        }

        [Fact]
        public async Task Subscribe()
        {
            Action<string> handler = Substitute.For<Action<string>>();
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);            
            _transport.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            await nats.Connect();
            nats.Subscribe("a", handler);
            _transport.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
        }

        [Fact]
        public async Task SubscribeThenConnect()
        {            
            Action<string> handler = Substitute.For<Action<string>>();
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _transport.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();            
            nats.Subscribe("a", handler);
            _transport.Received(0).Send(Arg.Any<string>());

            await nats.Connect();
            _transport.Received(1).Send(Arg.Is<string>(makeSubscription("a")));
        }

        
        string makeSubscription(string subject, string queue = " ")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("SUB {0} {1} {2}{3}", subject, queue, _sid++, System.Environment.NewLine);
            return sb.ToString();
        }


    }
}
