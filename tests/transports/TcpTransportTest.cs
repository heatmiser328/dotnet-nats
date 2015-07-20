using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Shouldly;

using mocks;

using dotnet_nats;

namespace transports
{
    public class TcpTransportTest
    {        
        MockTcpServer _server;
        ITransport _client;
        public TcpTransportTest()
        {                        
            _server = new MockTcpServer();
            _server.Start();
            _client = new TcpTransport("127.0.0.1", MockTcpServer.cPort);            
        }
        ~TcpTransportTest()
        {
            _client.Close();
            _server.Stop();
        }

        //void Open(string address = null, int port = 0);
        [Fact]
        public async Task Open()
        {            
            var connections = 0;
            Exception exception = null;
            //*
            _client.Error += new EventHandler<TransportErrorArgs>((sender, args) => {
                exception = args.Exception;
            });
            _client.Connected += new EventHandler<TransportConnectionArgs>((sender, args) => {
                args.Connected.ShouldBe(true);
                args.Reconnected.ShouldBe(false);
                connections++;                
            });
            var connected = await _client.Open();
            //*/ 
            connected.ShouldBe(true);
            connections.ShouldBe(1);
            exception.ShouldBe(null);
        }
        //void Close();
        //void Send(string data);
        //void Send(byte[] data);
        //void Flush();

    }
}
