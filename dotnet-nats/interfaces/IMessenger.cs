using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface IMessenger
    {
		event EventHandler<Message> Msg;
		event EventHandler Ping;
		event EventHandler Pong;
		event EventHandler Info;
		event EventHandler Error;
		
        void Receive(byte[] data, int size);
    }
}
