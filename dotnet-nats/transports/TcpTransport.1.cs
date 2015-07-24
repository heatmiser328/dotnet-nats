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
        const int cBufferSize = 256;
        string _address;
        int _port;
        Socket _socket;
        ConcurrentQueue<byte[]> _tosend;        

        public TcpTransport(string address, int port)
        {
            _address = address;
            _port = port;
            _tosend = new ConcurrentQueue<byte[]>();            
        }
        public TcpTransport() : this(null, -1) {}

        #region ITransport
        public event EventHandler<TransportConnectionArgs> Connected;
        public event EventHandler<TransportConnectionArgs> Reconnected;
        public event EventHandler<TransportConnectionArgs> Disconnected;
        public event EventHandler<TransportErrorArgs> Error;
        public event EventHandler<TransportDataArgs> Sent;
        public event EventHandler<TransportDataArgs> Received;

        public async Task<bool> Open(string address = null, int port = -1)
        {
            try
            {
                if (address != null) _address = address;
                if (port > 0) _port = port;
                
                IPEndPoint server = new IPEndPoint(IPAddress.Parse(_address), _port);
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
				args.SocketFlags = SocketFlags.None;
                args.RemoteEndPoint = server;
                var awaitable = new SocketAwaitable(args);
                awaitable.OnCompleted(() => {
                    RaiseConnected();
                    //Flush();                    
                    //Receive();
                });

                _socket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);                
                await _socket.ConnectAsync(awaitable);
                //await Receive();
                //await Flush();                
                return awaitable.IsCompleted;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public async Task<bool> Close()
        {
            try
            {
                if (_socket != null)
                {
                    //await Flush();
                    _socket.Shutdown(SocketShutdown.Both);
					SocketAsyncEventArgs args = new SocketAsyncEventArgs();            
                    var awaitable = new SocketAwaitable(args);
                    awaitable.OnCompleted(() => {
                        RaiseDisconnected();
                        // cancel pending receives
                    });
                    await _socket.DisconnectAsync(awaitable);
                    return awaitable.IsCompleted;
                }
                return true;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public async Task<bool> Send(string data)
        {
            try
            {
                byte[] b = Encoding.UTF8.GetBytes(data);
                return await Send(b);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public async Task<bool> Send(byte[] data)
        {
            try
            {
                _tosend.Enqueue(data);
                return await Flush();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public async Task<bool> Flush()
        {
            try
            {
                if (_socket == null)
                    return false;

                byte[] data;
                while (_tosend.TryDequeue(out data))
                {
                    SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                    args.SetBuffer(data, 0, data.Length);
                    var awaitable = new SocketAwaitable(args);                    
                    awaitable.OnCompleted(() => {
                        RaiseSent(awaitable.GetResult());                        
                    });
                    await _socket.SendAsync(awaitable);                    
                }
                return true;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        #endregion        

        #region Receive
        async Task<bool> Receive()
        {
            try
            {
                if (_socket == null)
                    return false;

				SocketAsyncEventArgs args = new SocketAsyncEventArgs();            
				args.SetBuffer(new byte[cBufferSize], 0, cBufferSize);
                var awaitable = new SocketAwaitable(args);
                List<byte> message = new List<byte>();
                int bytes;
                while ((bytes = await _socket.ReceiveAsync(awaitable)) > 0)
                {
                    message.AddRange(args.Buffer.Take(bytes).ToList());
                }
                RaiseReceived(message.ToArray());
                return await Receive();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                Receive();  // want to continue to receive...
                return false;
            }
        }

        #endregion
        
        #region Events
        void RaiseConnected()
        {
            if (Connected != null)
                Connected(this, new TransportConnectionArgs(true, false));
        }
        void RaiseReconnected()
        {
            if (Reconnected != null)
                Reconnected(this, new TransportConnectionArgs(true, true));
        }
        void RaiseDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, new TransportConnectionArgs(false, false));
        }        
        void RaiseError(Exception ex)
        {
            if (Error != null)
                Error(this, new TransportErrorArgs(ex));
        }        
        void RaiseSent(int sent)
        {
            if (Sent != null)
                Sent(this, new TransportDataArgs(null, sent));
        }
        void RaiseReceived(byte[] data)
        {
            if (Received != null)
                Received(this, new TransportDataArgs(data, data.Length));
        }
        #endregion

    }
}

/*
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace dotnet_nats
{
    public class TcpTransport : ITransport
    {
        string _address;
        int _port;
        Socket _socket;
        ConcurrentQueue<byte[]> _tosend;        

        public TcpTransport(string address, int port)
        {
            _address = address;
            _port = port;
            _tosend = new ConcurrentQueue<byte[]>();            
        }
        public TcpTransport() : this(null, -1) {}

        #region ITransport
        public event EventHandler<TransportConnectionArgs> Connected;
        public event EventHandler<TransportConnectionArgs> Reconnected;
        public event EventHandler<TransportConnectionArgs> Disconnected;
        public event EventHandler<TransportErrorArgs> Error;
        public event EventHandler<TransportDataArgs> Sent;
        public event EventHandler<TransportDataArgs> Received;

        public void Open(string address = null, int port = -1)
        {
            try
            {
                if (address != null) _address = address;
                if (port > 0) _port = port;

                IPEndPoint server = new IPEndPoint(IPAddress.Parse(_address), _port);
                _socket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);                                
                _socket.BeginConnect(server, new AsyncCallback(OnConnected), _socket);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public void Close()
        {
            try
            {
                if (_socket != null)
                {
                    Flush();
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.BeginDisconnect(true, new AsyncCallback(OnDisconnected), _socket);
                }            
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public void Send(string data)
        {
            try
            {
                byte[] b = Encoding.UTF8.GetBytes(data);
                Send(b);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public void Send(byte[] data)
        {
            try
            {
                _tosend.Enqueue(data);
                Flush();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public void Flush()
        {
            try
            {
                if (_socket == null)
                    return;

                byte[] data;
                if (_tosend.TryDequeue(out data))
                {
                    _socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSent), _socket);
                }       
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        #endregion        

        #region Receive
        class ReceiveState
        {
            public ReceiveState(Socket s) { socket = s; }
            public Socket socket;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize];
            public List<byte> message = new List<byte>();
        }

        void Receive(ReceiveState state = null)
        {
            try
            {
                if (_socket == null)
                    return;

                if (state == null)
                    state = new ReceiveState(_socket);
                _socket.BeginReceive(state.buffer, 0, ReceiveState.BufferSize, SocketFlags.None, new AsyncCallback(OnReceived), state);
            }
            catch (Exception ex)
            {
                RaiseError(ex);                
            }
        }


        #endregion
        
        #region Event Handlers
        void OnConnected(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                s.EndConnect(ar);
                Flush();
                RaiseConnected();
                Receive();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
        }

        void OnDisconnected(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                s.EndDisconnect(ar);
                RaiseDisconnected();
                // cancel pending receives
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
        }

        void OnSent(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                int sent = s.EndSend(ar);
                RaiseSent(sent);
                Flush();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
        }

        void OnReceived(IAsyncResult ar)
        {
            try
            {
                ReceiveState state = (ReceiveState)ar.AsyncState;
                int received = state.socket.EndReceive(ar);
                if (received > 0)
                {                    
                    state.message.AddRange(state.buffer.Take(received).ToList());
                    Receive(state); // read the remaining
                }
                else
                {
                    RaiseReceived(state.message.ToArray());
                    Receive();
                }                
            }
            catch (Exception ex)
            {
                RaiseError(ex);
            }
        }

        #endregion 

        #region Events
        void RaiseConnected()
        {
            if (Connected != null)
                Connected(this, new TransportConnectionArgs(true, false));
        }
        void RaiseReconnected()
        {
            if (Reconnected != null)
                Reconnected(this, new TransportConnectionArgs(true, true));
        }
        void RaiseDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, new TransportConnectionArgs(false, false));
        }        
        void RaiseError(Exception ex)
        {
            if (Error != null)
                Error(this, new TransportErrorArgs(ex));
        }        
        void RaiseSent(int sent)
        {
            if (Sent != null)
                Sent(this, new TransportDataArgs(null, sent));
        }
        void RaiseReceived(byte[] data)
        {
            if (Received != null)
                Received(this, new TransportDataArgs(data, data.Length));
        }
        #endregion

    }
}
*/