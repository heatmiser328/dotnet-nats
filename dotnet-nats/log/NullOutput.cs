using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats.log
{
    public class NullOutput : IOutput
    {
        public void Out(string l, string s)
        {            
        }
    }
}
