using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class TransportConnectionArgs : EventArgs
    {
        public TransportConnectionArgs(bool connected, bool reconnected) { Connected = connected; Reconnected = reconnected; }
        public bool Connected {get; private set;}
        public bool Reconnected {get; private set;}
    }

    public class TransportErrorArgs : EventArgs
    {
        public TransportErrorArgs(Exception ex) { Exception = ex; }
        public Exception Exception { get; private set; }
    }

    public class TransportDataArgs : EventArgs
    {
        public TransportDataArgs(byte[] data, int size) { Data = data; Size = size; }
        public byte[] Data { get; private set; }
        public int Size { get; private set; }
    }

    public interface ITransport
    {
		event EventHandler<TransportConnectionArgs> Connected;
		event EventHandler<TransportConnectionArgs> Reconnected;
		event EventHandler<TransportConnectionArgs> Disconnected;
		event EventHandler<TransportErrorArgs> Error;
		event EventHandler<TransportDataArgs> Sent;
        event EventHandler<TransportDataArgs> Received;
		
		Task<bool> Open(string address = null, int port = 0);
        Task<bool> Close();
        Task<bool> Send(string data);
        Task<bool> Send(byte[] data);
        Task<bool> Flush();
    }
}
