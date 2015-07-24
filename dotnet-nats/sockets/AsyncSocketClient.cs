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
    public class SocketDataArgs : EventArgs
    {
        public SocketDataArgs(byte[] data, int size) { Data = data; Size = size; }
        public byte[] Data { get; private set; }
        public int Size { get; private set; }
    }

    public class AsyncSocketClient
    {
        string _address;
        int _port;
        Socket _socket;
        ConcurrentQueue<byte[]> _tosend;        

        public AsyncSocketClient(string address, int port)
        {
            _address = address;
            _port = port;
            _tosend = new ConcurrentQueue<byte[]>();            
        }
        public AsyncSocketClient() : this(null, -1) { }

        public event EventHandler<bool> Connected;
        public event EventHandler<bool> Reconnected;
        public event EventHandler<bool> Disconnected;
        public event EventHandler<Exception> Error;
        public event EventHandler<int> Sent;
        public event EventHandler<SocketDataArgs> Received;

        public Task<bool> Open(string address = null, int port = -1)
        {
            try
            {
                if (address != null) _address = address;
                if (port > 0) _port = port;

                IPEndPoint server = new IPEndPoint(IPAddress.Parse(_address), _port);
                _socket = new Socket(server.AddressFamily, SocketType.Stream, ProtocolType.Tcp);                                
				var tcs = new TaskCompletionSource<bool>(_socket); 
                _socket.BeginConnect(server, (ar) => {
		            try
		            {
						var t = (TaskCompletionSource<bool>)ar.AsyncState; 
						var s = (Socket)t.Task.AsyncState; 
						try {
                            s.EndConnect(ar);
							t.TrySetResult(ar.IsCompleted); 
			                RaiseConnected();
			                Flush();
							Receive();
						} 
						catch (Exception exc) { 
							RaiseError(exc);
							t.TrySetException(exc); 
						} 
		            }
		            catch (Exception ex)
		            {
		                RaiseError(ex);
		            }
				}, tcs);
				
				return tcs.Task;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public Task<bool> Close()
        {
            try
            {
                if (_socket != null && _socket.Connected)
                {    
                    return Flush().ContinueWith((antecedent) => {
                        _socket.Shutdown(SocketShutdown.Both);
                        var tcs = new TaskCompletionSource<bool>(_socket);
                        _socket.BeginDisconnect(true, (ar) =>
                        {
                            try
                            {
                                var t = (TaskCompletionSource<bool>)ar.AsyncState;
                                var s = (Socket)t.Task.AsyncState;
                                try
                                {
                                    s.EndDisconnect(ar);
                                    t.TrySetResult(false);
                                    RaiseDisconnected();
                                    // cancel pending receives
                                }
                                catch (Exception exc)
                                {
                                    RaiseError(exc);
                                    t.TrySetException(exc);
                                }
                            }
                            catch (Exception ex)
                            {
                                RaiseError(ex);
                            }
                        }, tcs);

                        return tcs.Task;
                    }).Result;
                }            
				return Task<bool>.FromResult(false);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public Task<int> Send(string data)
        {
            try
            {
                byte[] b = Encoding.UTF8.GetBytes(data);
                return Send(b);
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public Task<int> Send(byte[] data)
        {
            try
            {
                _tosend.Enqueue(data);
                return Flush();
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public Task<int> Flush()
        {
            try
            {
                if (_socket == null)
                    return Task<int>.FromResult(0);
                if (_tosend.IsEmpty)
                    return Task<int>.FromResult(0);
                
            	var tcs = new TaskCompletionSource<int>(_socket);
                List<byte> send = new List<byte>();
                byte[] data;
                while (_tosend.TryDequeue(out data))
                {
                    send.AddRange(data);                    
                }
                if (send.Count > 0)
                {
                    data = send.ToArray();
                    _socket.BeginSend(data, 0, data.Length, SocketFlags.None, (ar) => {
			            try
			            {
							var t = (TaskCompletionSource<int>)ar.AsyncState; 
							var s = (Socket)t.Task.AsyncState; 
							try {
                                int sent = s.EndSend(ar);
								t.TrySetResult(sent); 
				                RaiseSent(sent);
							} 
							catch (Exception exc) { 
								RaiseError(exc);
								t.TrySetException(exc); 
							} 
			            }
			            catch (Exception ex)
			            {
			                RaiseError(ex);
			            }
					}, tcs);					
                }
				else
				{
					tcs.TrySetResult(0);					
				}
                return tcs.Task;				
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                throw;
            }
        }

        public Task<byte[]> Receive()
		{
            return DoReceive();
		}
		
        #region Receive
        class ReceiveState
        {
            public ReceiveState(Socket s) { socket = s; Task = new TaskCompletionSource<byte[]>(socket);}
            public Socket socket;
            public TaskCompletionSource<byte[]> Task;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize];
            public List<byte> message = new List<byte>();
        }

        Task<byte[]> DoReceive(ReceiveState state = null)
        {
            try
            {
                if (_socket == null)
                    return Task<byte[]>.FromResult(new byte[]{});

                if (state == null)
                    state = new ReceiveState(_socket);
                _socket.BeginReceive(state.buffer, 0, ReceiveState.BufferSize, SocketFlags.None, (ar) => {
		            try
		            {
		                ReceiveState st = (ReceiveState)ar.AsyncState;
                        var t = st.Task;
                        var s = (Socket)t.Task.AsyncState;
                        try
                        {
                            int received = st.socket.EndReceive(ar);
                            if (received > 0)
                            {
                                state.message.AddRange(st.buffer.Take(received).ToList());
                                DoReceive(state).Wait(); // read the remaining
                            }
                            else
                            {
                                var data = st.message.ToArray();
                                t.TrySetResult(data);
                                RaiseReceived(data, data.Length);
                            }                                                                    
                        }
                        catch (Exception exc)
                        {
                            RaiseError(exc);
                            t.TrySetException(exc);
                        } 
		            }
		            catch (Exception ex)
		            {
		                RaiseError(ex);
		            }
				}, state);
                return state.Task.Task;
            }
            catch (Exception ex)
            {
                RaiseError(ex);
                return Task<byte[]>.FromResult(new byte[] { });
            }
        }
        #endregion
        
        #region Events
        void RaiseConnected()
        {
            if (Connected != null)
                Connected(this, true);
        }
        void RaiseReconnected()
        {
            if (Reconnected != null)
                Reconnected(this, true);
        }
        void RaiseDisconnected()
        {
            if (Disconnected != null)
                Disconnected(this, false);
        }        
        void RaiseError(Exception ex)
        {
            if (Error != null)
                Error(this, ex);
        }
        void RaiseSent(int length)
        {
            if (Sent != null)
                Sent(this, length);
        }
        void RaiseReceived(byte[] data, int length)
        {
            if (Received != null)
                Received(this, new SocketDataArgs(data, length));
        }
        #endregion
    }
}
