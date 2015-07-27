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
    public class ConnectivityTests
    {
        ILog _log;
        IServerFactory _factory;
        IServer _server;
        ITransport _transport;
        ICollection<IServer> _servers;
        IDictionary<string, object> _opts;
        const string cURL = "nats://domain:4222";
        const string cConnect = @"CONNECT {""verbose"":false,""pedantic"":false}" + "\r\n";

        public ConnectivityTests()
        {
            _opts = new Dictionary<string, object>();
            _opts.Add("url", cURL);
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
        public void Connect()
        {
            _transport
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _transport.Connected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(true)); });

            _server.Connected.Returns(false, true);
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            nats.Connect().ShouldBe(true);            
            nats.Connected.ShouldBe(true);
            // this checks subscriptions, not events being raised to the subscribers!
            _transport.Received().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();            
            _transport.Received().Open();
            _transport.Received().Send(Arg.Is<string>(cConnect));
        }

        [Fact]
        public void Connect_NoServer()
        {
            _server.Connected.Returns(false);
            _opts.Clear();
            _servers.Clear();
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(0);
            nats.Connected.ShouldBe(false);
            nats.Connect().ShouldBe(false);
            nats.Connected.ShouldBe(false);
            _transport.DidNotReceive().Open();
            _transport.DidNotReceive().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();            
            _transport.DidNotReceive().Send(Arg.Any<string>());            
        }

        [Fact]
        public void Connect_Fail()
        {            
            _server.Connected.Returns(false);
            //_transport.WhenForAnyArgs(x => x.Open()).Do(x => { throw new System.Net.Sockets.SocketException(10061); });
            _transport.Error += Raise.EventWith(new dotnet_sockets.EventArgs<Exception>(new System.Net.Sockets.SocketException(10061)));            

            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            nats.Connect().ShouldBe(true);
            nats.Connected.ShouldBe(false);
            _transport.Received().Open();
            _transport.DidNotReceive().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();            
            _transport.DidNotReceive().Send(Arg.Any<string>());
            _transport.Received().Error += Arg.Any<EventHandler<dotnet_sockets.EventArgs<Exception>>>();            
        }

    }
}
