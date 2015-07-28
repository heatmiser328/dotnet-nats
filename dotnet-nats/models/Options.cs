using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class Options
    {
        public bool verbose { get; set; }
        public bool pedantic { get; set; }
        public int reconnectDelay { get; set; }
        public List<string> uris { get; set; }
    }
}
