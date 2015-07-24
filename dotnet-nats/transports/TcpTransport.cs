using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using dotnet_nats.sockets;

namespace dotnet_nats
{
    public class TcpTransport : ITransport
    {
		AsyncSocketClient _client;
        public TcpTransport(string address, int port)
        {
			_client = new AsyncSocketClient(address, port);
        }
        public TcpTransport() : this(null, -1) {}

        #region ITransport
        public event EventHandler<bool> Connected
		{
            add { _client.Connected += value; }
            remove { _client.Connected -= value; }
		}
        public event EventHandler<bool> Reconnected
		{
            add { _client.Reconnected += value; }
            remove { _client.Reconnected -= value; }
		}
        public event EventHandler<bool> Disconnected
		{
            add { _client.Disconnected += value; }
            remove { _client.Disconnected -= value; }
		}
        public event EventHandler<Exception> Error
		{
            add { _client.Error += value; }
            remove { _client.Error -= value; }
		}
        public event EventHandler<int> Sent
		{
            add { _client.Sent += value; }
            remove { _client.Sent -= value; }
		}
        public event EventHandler<SocketDataArgs> Received
		{
            add { _client.Received += value; }
            remove { _client.Received -= value; }
		}
		
        public async Task<bool> Open(string address = null, int port = -1)
        {
			return await _client.Open(address, port);
        }

        public async Task<bool> Close()
        {
            return await _client.Close();
        }

        public async Task<int> Send(string data)
        {
            return await _client.Send(data);
        }

        public async Task<int> Send(byte[] data)
        {
            return await _client.Send(data);
        }

        public async Task<int> Flush()
        {
            return await Flush();
        }

        public async Task<byte[]> Receive()
        {
            return await _client.Receive();
        }

        #endregion        
    }
}
