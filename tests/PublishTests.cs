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
    public class PublishTests
    {
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

        public PublishTests()
        {
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
        public async Task Publish()
        {
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _transport.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            await nats.Connect();
            nats.Publish("a", "data");
            _transport.Received(1).Send(Arg.Is<string>(makePublication("a", "data")));
            _log.Received().Trace("Sent {0} bytes to server @ {1}", 1, _server.URL);            
        }

        [Fact]
        public async Task PublishMultiple()
        {
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _transport.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            await nats.Connect();
            nats.Publish("a", "data1");
            nats.Publish("a", "data2");
            nats.Publish("b", "data");
            _transport.Received(1).Send(Arg.Is<string>(makePublication("a", "data1")));
            _transport.Received(1).Send(Arg.Is<string>(makePublication("a", "data2")));
            _transport.Received(1).Send(Arg.Is<string>(makePublication("b", "data")));
            _log.Received(3).Trace("Sent {0} bytes to server @ {1}", 1, _server.URL);            
        }

        [Fact]
        public async Task PublishConfirm()
        {
            Action<string> handler = Substitute.For<Action<string>>();
            byte[] pong = Encoding.UTF8.GetBytes(PONG);
            _msgr.When(x => x.Receive(Arg.Is<byte[]>(pong), Arg.Is<int>(pong.Length)))
                .Do(x => { handler(""); });
            _transport
                .When(x => x.Send(Arg.Is<string>("PING\r\n")))
                .Do(x => { _transport.ReceivedData += Raise.EventWith(new dotnet_sockets.SocketDataArgs(null, pong, pong.Length)); });                
                                            
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            _transport.Received().Sent += Arg.Any<EventHandler<dotnet_sockets.EventArgs<int>>>();
            _transport.Received().ReceivedData += Arg.Any<EventHandler<dotnet_sockets.SocketDataArgs>>();
            await nats.Connect();
            nats.Publish("a", "data", handler);
            _transport.Received(1).Send(Arg.Is<string>(makePublication("a", "data")));
            _transport.Received(1).Send(Arg.Is<string>("PING\r\n"));            
            _log.Received(2).Trace("Sent {0} bytes to server @ {1}", 1, _server.URL);
            handler.ReceivedCalls().Count().ShouldBe(1);
        }

        string makePublication(string subject, string data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("PUB {0} {1}{2}{3}{2}", subject, data.Length, System.Environment.NewLine, data);
            return sb.ToString();
        }
    }
}
