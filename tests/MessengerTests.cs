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
    public class MessengerTests
    {
        ILog _log;        
        Options _opts;
        const string cConnect = @"CONNECT {""verbose"":false,""pedantic"":false}" + "\r\n";

        public MessengerTests()
        {
            _log = Substitute.For<ILog>();            
            _opts = new Options();
        }

        [Fact]
        public void Instantiate()
        {
            Messenger msgr = new Messenger(_log);
            msgr.ShouldNotBe(null);
        }

        [Fact]
        public void ReceiveControlPong()
        {
            byte[] pong = Encoding.UTF8.GetBytes("PONG\r\n");

            Action<string> handler = Substitute.For<Action<string>>();
            Messenger msgr = new Messenger(_log);
            msgr.ShouldNotBe(null);            
            msgr.Ping(handler);            

            msgr.Receive(pong, pong.Length);
            handler.ReceivedCalls().Count().ShouldBe(1);
        }
    }
}
