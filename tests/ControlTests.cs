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
    public class ControlTests
    {
        ILog _log;
        IFactory _factory;
        IServer _server;        
        IMessenger _msgr;
        ICollection<IServer> _servers;
        Options _opts;
        const string cURL = "nats://domain:4222";
        const string PING = "PING\r\n";
        const string PONG = "PONG\r\n";

        public ControlTests()
        {
            _log = Substitute.For<ILog>();
            _opts = new Options();            
            _opts.uris = new List<string>() {cURL};
            _servers = new List<IServer>();
            
            _msgr = Substitute.For<IMessenger>();

            _server = Substitute.For<IServer>();
            _server.IsConnected.Returns(true);
            _server.URL.Returns(cURL);
            _server.Open().Returns(Task<bool>.FromResult(true));
            _server.When(x => x.Close())
                .Do(x => { _server.Disconnected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(false)); });
            _server.Send(Arg.Any<string>())
                .Returns(Task<int>.FromResult(1))
                .AndDoes(x => { _server.Sent += Raise.EventWith(new dotnet_sockets.EventArgs<int>(1)); });            
            _servers.Add(_server);
            _factory = Substitute.For<IFactory>();
            _factory.NewServer(Arg.Any<string[]>()).Returns(_servers);
            _factory.NewMessenger().Returns(_msgr);
        }

        [Fact]
        public void Ping()
        {
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);            
            _msgr.Ping += Raise.Event();            
            _server.Received(1).Send(Arg.Is<string>(PONG));
            _log.Received().Trace("Sent {0} bytes to server @ {1}", 1, _server.URL);
        }

        [Fact]
        public void Error()
        {
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);

            _msgr.Error += Raise.Event();

            _server.Received(1).Close();
            _log.Received(1).Info("Disconnecting from Server @ {0}", _server.URL);
            _log.Received(1).Warn("Disconnected from server @ {0}", _server.URL);
            _log.Received(1).Debug("Reconnecting to server");
            _server.Received(1).Open();
        }

    }
}
