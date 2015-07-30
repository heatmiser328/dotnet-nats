using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_sockets;

namespace dotnet_nats
{
    public interface IServer
    {
        string URL { get; }
        string Address { get; }
        int Port { get; }
        bool Connected { get; }
        int ReconnectAttempts { get; set; }

        event EventHandler<EventArgs<bool>> Connected;        
        event EventHandler<EventArgs<bool>> Disconnected;
        event EventHandler<EventArgs<Exception>> Error;
        event EventHandler<EventArgs<int>> Sent;
        event EventHandler<SocketDataArgs> ReceivedData;              

        Task<bool> Open();
        Task<bool> Close();
        Task<int> Send(string data);
        Task<int> Flush();
        Task<byte[]> Receive();
    }
}
