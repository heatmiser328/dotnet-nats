using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Shouldly;
using NSubstitute;

using dotnet_nats;
using dotnet_nats.log;

namespace tests
{
    public class LogTests
    {
        IOutput _output;

        public LogTests()
        {
            _output = Substitute.For<IOutput>();
        }

        [Fact]
        public void Trace()
        {
            ILog log = new Logger(_output, "trace");
            log.Trace("trace this: {0}", 1);
            log.Debug("debug this: {0}", 1);

            _output.Received(1).Out("TRACE", Arg.Any<string>());
            _output.Received(1).Out("DEBUG", Arg.Any<string>());
        }

        [Fact]
        public void Debug()
        {
            ILog log = new Logger(_output, "debug");
            log.Trace("trace this: {0}", 1);
            log.Debug("debug this: {0}", 1);
            log.Error("error this: {0}", 1);

            _output.Received(0).Out("TRACE", Arg.Any<string>());
            _output.Received(1).Out("DEBUG", Arg.Any<string>());
            _output.Received(1).Out("ERROR", Arg.Any<string>());
        }

    }
}
