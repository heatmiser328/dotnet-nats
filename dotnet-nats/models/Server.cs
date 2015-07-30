using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_sockets;

namespace dotnet_nats
{
    public class Server : IServer
    {
        ILog _log;
        ISocketClient _client;

        public Server(ILog log) 
        {
            _log = log;
            _client = new AsyncSocketClient();
            _client.Log += (sender, args) => {
                _log.Log(args.Level, args.Message, args.Exception, args.Args);
            };
            ReconnectAttempts = 0; 
        }
        public string URL { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
        public bool IsConnected { get { return _client != null && _client.IsConnected; } }
        public int ReconnectAttempts { get; set; }

        public event EventHandler<EventArgs<bool>> Connected
        {
            add { _client.Connected += value; }
            remove { _client.Connected -= value; }
        }      
        public event EventHandler<EventArgs<bool>> Disconnected
        {
            add { _client.Disconnected += value; }
            remove { _client.Disconnected -= value; }
        }
        public event EventHandler<EventArgs<Exception>> Error
        {
            add { _client.Error += value; }
            remove { _client.Error -= value; }
        }
        public event EventHandler<EventArgs<int>> Sent
        {
            add { _client.Sent += value; }
            remove { _client.Sent -= value; }
        }
        public event EventHandler<SocketDataArgs> ReceivedData
        {
            add { _client.ReceivedData += value; }
            remove { _client.ReceivedData -= value; }
        }

        public Task<bool> Open()
        {                            
            return _client.Open(this.Address, this.Port);
        }
        public Task<bool> Close()
        {
            return _client.Close();
        }
        public Task<int> Send(string data)
        {
            return _client.Send(data);
        }
        public Task<int> Flush()
        {
            return _client.Flush();
        }
        public Task<byte[]> Receive()
        {
            return _client.Receive();
        }        
    }
}
