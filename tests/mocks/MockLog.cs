using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mocks
{
    internal class MockLog : dotnet_nats.ILog
    {
        readonly Xunit.Abstractions.ITestOutputHelper output;
        public MockLog(Xunit.Abstractions.ITestOutputHelper output)
        {
            this.output = output;
        }

        public string Level {get;set;}

        public void Trace(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Debug(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Info(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Warn(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Error(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Error(string msg, Exception ex, params object[] args)
        {
            output.WriteLine(msg, args);
        }

        public void Fatal(string msg, params object[] args)
        {
            output.WriteLine(msg, args);
        }
    }
}
