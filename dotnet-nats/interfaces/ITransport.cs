using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface ITransport
    {
		event EventHandler<bool> Connected;
		event EventHandler<bool> Reconnected;
		event EventHandler<bool> Disconnected;
		event EventHandler<Exception> Error;
		event EventHandler<int> Sent;
        event EventHandler<SocketDataArgs> Received;
		
		Task<bool> Open(string address = null, int port = 0);
        Task<bool> Close();
        Task<int> Send(string data);
        Task<int> Send(byte[] data);
        Task<int> Flush();
        Task<byte[]> Receive();
    }
}
