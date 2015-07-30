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
    public class MessageTests
    {
        [Fact]
        public void Parse()
        {
            string mh = "MSG a 1   68\r\n";
            var msg = Message.Parse(mh);
            msg.ShouldNotBe(null);
            msg.Subject.ShouldBe("a");
            msg.SubscriptionID.ShouldBe(1);
            msg.ReplyTo.ShouldBe("");
            msg.Size.ShouldBe(68);
            msg.Data.ShouldBe(null);
        }

        [Fact]
        public void ParseReply()
        {
            string mh = "MSG abc 34 me 168\r\n";
            var msg = Message.Parse(mh);
            msg.ShouldNotBe(null);
            msg.Subject.ShouldBe("abc");
            msg.SubscriptionID.ShouldBe(34);
            msg.ReplyTo.ShouldBe("me");
            msg.Size.ShouldBe(168);
            msg.Data.ShouldBe(null);
        }
    }
}
