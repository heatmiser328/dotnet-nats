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
        public void ReceiveControlPing()
        {
            byte[] ping = Encoding.UTF8.GetBytes("PING\r\n");
            var pinged = false;            
            Messenger msgr = new Messenger(_log);
            msgr.Ping += (s, a) => { pinged = true; };
            msgr.ShouldNotBe(null);                        

            msgr.Receive(ping, ping.Length);
            pinged.ShouldBe(true);
        }

        [Fact]
        public void ReceiveControlPong()
        {
            byte[] pong = Encoding.UTF8.GetBytes("PONG\r\n");
            var ponged = false;            
            Messenger msgr = new Messenger(_log);
            msgr.Pong += (s, a) => { ponged = true; };
            msgr.ShouldNotBe(null);

            msgr.Receive(pong, pong.Length);
            ponged.ShouldBe(true);
        }

        [Fact]
        public void ReceiveMsg()
        {
            const string Message = "This is a message";
            byte[] payload = Encoding.UTF8.GetBytes(Message + "\r\n");
            byte[] header = Encoding.UTF8.GetBytes("MSG a 1   " + payload.Length.ToString() + "\r\n");            
            byte[] msg = header.Concat(payload).ToArray();
            Messenger msgr = new Messenger(_log);
			Message message = null;
            msgr.Msg += (s, a) => { message = a; };
            msgr.ShouldNotBe(null);

            msgr.Receive(msg, msg.Length);
            message.Data.ShouldBe(Message);
        }
		
    }
}
