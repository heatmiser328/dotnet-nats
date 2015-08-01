using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats.log
{
    public class ConsoleOutput : IOutput
    {
        public void Out(string level, string msg)
        {
			Console.Out.WriteLine("{0}: {1,-5}: {2}", DateTime.Now, level, msg);
        }
    }
}
