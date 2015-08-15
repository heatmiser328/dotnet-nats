using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;
using Shouldly;
using NSubstitute;

using dotnet_nats;

#pragma warning disable 4014

namespace tests
{
    public class MessengerTests
    {
        readonly ITestOutputHelper output;
        ILog _log;        
        Options _opts;

        public MessengerTests(ITestOutputHelper output)
        {
            this.output = output;
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
            message.ShouldNotBe(null);
            message.Data.ShouldBe(Message);
        }

        [Fact]
        public void ReceiveMsgWithEmbeddedCRLF()
        {
            const string Message = "This is a message\r\nwith some\r\nembedded characters\r\n";
            byte[] payload = Encoding.UTF8.GetBytes(Message + "\r\n");
            byte[] header = Encoding.UTF8.GetBytes("MSG a 1   " + payload.Length.ToString() + "\r\n");
            byte[] msg = header.Concat(payload).ToArray();
            Messenger msgr = new Messenger(_log);
            Message message = null;
            msgr.Msg += (s, a) => { message = a; };
            msgr.ShouldNotBe(null);

            msgr.Receive(msg, msg.Length);
            message.ShouldNotBe(null);
            message.Data.ShouldBe(Message);
        }

        [Fact]
        public void ReceiveMsgMultiple()
        {
            string xml = System.IO.File.ReadAllText(System.IO.Path.Combine(helpers.TestPaths.TestFolder, "FUBAR10k.xml"));            
            byte[] payload = Encoding.UTF8.GetBytes(xml + "\r\n");
            byte[] header = Encoding.UTF8.GetBytes("MSG a 1   " + payload.Length.ToString() + "\r\n");
            byte[] msg = header.Concat(payload).ToArray();
            Messenger msgr = new Messenger(_log);            
            Queue<Message> messages = new Queue<Message>();
            msgr.Msg += (s, a) => { messages.Enqueue(a); };
            msgr.ShouldNotBe(null);

            Task[] tasks = new Task[100];
            for (int i = 0; i < 100; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {                    
                    msgr.Receive(msg, msg.Length);
                });
            }
            Task.WaitAll(tasks);
            _log.Received(0).Warn(Arg.Any<string>());
            messages.Count.ShouldBe(100);
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                message.ShouldNotBe(null);
                message.Data.ShouldBe(xml);
            }
        }

        [Fact]
        public void ReceiveMsgPartial()
        {
            const string Message = "This is a message\r\nwith some\r\nembedded characters\r\n";
            byte[] payload = Encoding.UTF8.GetBytes(Message + "\r\n");
            byte[] header = Encoding.UTF8.GetBytes("MSG a 1   " + payload.Length.ToString() + "\r\n");
            List<byte> msg = header.Concat(payload).ToList();
            Messenger msgr = new Messenger(_log);
            Message message = null;
            msgr.Msg += (s, a) => { message = a; };
            msgr.ShouldNotBe(null);

            byte[] part1 = msg.Take(msg.Count / 2).ToArray();
            msg.RemoveRange(0, msg.Count / 2);
            byte[] part2 = msg.Take(msg.Count).ToArray();
            msgr.Receive(part1, part1.Length);
            message.ShouldBe(null);
            msgr.Receive(part2, part2.Length);
            message.ShouldNotBe(null);
            message.Data.ShouldBe(Message);
        }

        [Fact]
        public void ReceiveMsgMultiplePartial()
        {            
            string xml = System.IO.File.ReadAllText(System.IO.Path.Combine(helpers.TestPaths.TestFolder, "FUBAR10k.xml"));
            byte[] payload = Encoding.UTF8.GetBytes(xml + "\r\n");
            byte[] header = Encoding.UTF8.GetBytes("MSG a 1   " + payload.Length.ToString() + "\r\n");
            List<byte> msg = header.Concat(payload).ToList();
            byte[] part1 = msg.Take(msg.Count / 2).ToArray();
            msg.RemoveRange(0, msg.Count / 2);
            byte[] part2 = msg.Take(msg.Count).ToArray();
            const int Count = 200;
            Queue<Message> messages = new Queue<Message>();
            Messenger msgr = new Messenger(_log);            
            msgr.Msg += (s, a) => { messages.Enqueue(a); };
            msgr.ShouldNotBe(null);

            //Task[] tasks = new Task[Count];
            for (int i = 0; i < Count; i++)
            {
                int idx = i;
                if (idx == Count - 1)
                    idx = i;
                //tasks[i] = Task.Factory.StartNew(() =>
                //{
                    if (idx % 2 == 0) 
                        msgr.Receive(part1, part1.Length);
                    else 
                        msgr.Receive(part2, part2.Length);
                //});
            }
            //Task.WaitAll(tasks);
            _log.Received(0).Warn(Arg.Any<string>());
            messages.Count.ShouldBe(100);
            while (messages.Count > 0)
            {
                Message message = messages.Dequeue();
                message.ShouldNotBe(null);
                message.Data.ShouldBe(xml);
            }
        }

    }
}
