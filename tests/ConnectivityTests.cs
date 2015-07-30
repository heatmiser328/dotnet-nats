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
    public class ConnectivityTests
    {
        ILog _log;
        IFactory _factory;
        IServer _server;        
        ICollection<IServer> _servers;
        Options _opts;
        const string cURL = "nats://domain:4222";
        const string cConnect = @"CONNECT {""verbose"":false,""pedantic"":false}" + "\r\n";

        public ConnectivityTests()
        {
            _opts = new Options();            
            _opts.uris = new List<string>() {cURL};
            _servers = new List<IServer>();
            _log = Substitute.For<ILog>();
                        
            _server = Substitute.For<IServer>();
            _server.URL.Returns(cURL);
            _server.Open().Returns(Task<bool>.FromResult(true));
            _servers.Add(_server);
            _factory = Substitute.For<IFactory>();
            _factory.NewServer(Arg.Any<string[]>()).Returns(_servers);            
        }

        [Fact]
        public async Task Connect()
        {
            _server
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _server.Connected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(true)); });

            _server.IsConnected.Returns(false, true);
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            var b = await nats.Connect();
            b.ShouldBe(true);            
            nats.Connected.ShouldBe(true);
            _server.Received().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received().Open();
            _server.Received().Send(Arg.Is<string>(cConnect));
        }

        [Fact]
        public async Task Connect_Handler()
        {
            var connected = false;
            _server
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _server.Connected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(true)); });

            _server.IsConnected.Returns(false, true);
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            var c = await nats.Connect((b) => connected = b);
            c.ShouldBe(true);
            connected.ShouldBe(true);
            connected.ShouldBe(true);
            nats.Connected.ShouldBe(true);
            _server.Received().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received().Open();
            _server.Received().Send(Arg.Is<string>(cConnect));
        }


        [Fact]
        public async Task Connect_NoServer()
        {
            _server.IsConnected.Returns(false);
            _opts.uris = new List<string>() { };
            _servers.Clear();
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(0);
            nats.Connected.ShouldBe(false);
            var c = await nats.Connect();
            c.ShouldBe(false);
            nats.Connected.ShouldBe(false);
            _server.DidNotReceive().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.DidNotReceive().Open();
            _server.DidNotReceive().Send(Arg.Any<string>());            
        }

        [Fact]
        public async Task Connect_Fail()
        {            
            _server.IsConnected.Returns(false);
            _server
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _server.Error += Raise.EventWith(new dotnet_sockets.EventArgs<Exception>(new System.Net.Sockets.SocketException(10061))); });
            //_transport.WhenForAnyArgs(x => x.Open()).Do(x => { throw new System.Net.Sockets.SocketException(10061); });
                        
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            var c = await nats.Connect();
            c.ShouldBe(true);
            nats.Connected.ShouldBe(false);
            _server.Received().Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received().Error += Arg.Any<EventHandler<dotnet_sockets.EventArgs<Exception>>>();
            _server.Received().Open();
            _log.Received().Error("Error with server @ {0}", cURL, Arg.Any<Exception>());
            _server.DidNotReceive().Send(Arg.Any<string>());            
        }

        [Fact]
        public async Task Reconnect()
        {
            _server
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _server.Connected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(true)); });

            _server.IsConnected.Returns(false, true);
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            var c = await nats.Connect();
            c.ShouldBe(true);
            _server.Disconnected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(false));

            nats.Connected.ShouldBe(true);
            _server.Received(1).Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received(1).Disconnected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received(2).Open();
            _server.Received(2).Send(Arg.Is<string>(cConnect));            
            _log.Received(1).Warn("Disconnected from server @ {0}", cURL);
            _log.Received(1).Debug("Reconnecting to server");
        }

        [Fact]
        public async Task Close()
        {
            _server
                .WhenForAnyArgs(x => x.Open())
                .Do(x => { _server.Connected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(true)); });
            _server
                .WhenForAnyArgs(x => x.Close())
                .Do(x => { _server.Disconnected += Raise.EventWith(new dotnet_sockets.EventArgs<bool>(false)); });

            _server.IsConnected.Returns(false, true);
            INATS nats = new NATS(_factory, _opts, _log);
            nats.ShouldNotBe(null);
            nats.Servers.ShouldBe(1);
            nats.Connected.ShouldBe(false);
            var c = await nats.Connect();
            c.ShouldBe(true);            
            nats.Connected.ShouldBe(true);
            _server.Received(1).Connected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();
            _server.Received(1).Disconnected += Arg.Any<EventHandler<dotnet_sockets.EventArgs<bool>>>();

            nats.Close();

            _server.Received(1).Open();
            _server.Received(1).Send(Arg.Is<string>(cConnect));
            _server.Received(1).Close();            
        }

    }
}
