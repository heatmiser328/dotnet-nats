using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class Subscription
    {
		static int sSID = 1;
    	public Subscription(int sid, string subject, Action<string> handler, string queue = null)
		{
			SID = sid;
			Subject = subject;
			Handler = handler;
			Queue = queue;
		}
    	public Subscription(string subject, Action<string> handler, string queue = null) : this(sSID++, subject, handler, queue)
		{
		}
	
		public int SID {get; private set;}
		public string Subject {get; private set;}
		public Action<string> Handler {get; private set;}
		public string Queue {get; private set;}
		
    }
}
