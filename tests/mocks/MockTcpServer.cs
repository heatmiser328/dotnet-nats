using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace mocks
{
    internal class MockTcpServer
    {
        public const int cPort = 8877;
        int _port = cPort;
        TcpListener _listener;

        public MockTcpServer(int port = cPort)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
            Reset();
        }

        public int Connections { get; private set; }
        public IList<byte[]> Received { get; private set; }

        public void Reset()
        {
            System.Diagnostics.Trace.WriteLine("MockTcpServer.Reset");
            Connections = 0;
            Received = new List<byte[]>();
        }

        public void Start()
        {
            System.Diagnostics.Trace.WriteLine("MockTcpServer.Start: Start listener");
            _listener.Start();
            Connect();            
        }

        public void Stop()
        {
            System.Diagnostics.Trace.WriteLine("MockTcpServer.Stop: Stop listener");
            _listener.Stop();
        }

        private void Connect()
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("MockTcpServer.Connect: Wait for socket connection");
                _listener.BeginAcceptSocket((ar) => {
                    TcpListener l = (TcpListener)ar.AsyncState;
                    Socket s = l.EndAcceptSocket(ar);
                    System.Diagnostics.Trace.WriteLine("MockTcpServer.Connect: Socket connected");
                    Receive(new ReceiveState(s));
                    Connect();
                }, _listener);
            }
            catch{}
        }

        private class ReceiveState
        {
            public ReceiveState(Socket s) { socket = s; }
            public Socket socket;
            public const int BufferSize = 256;
            public byte[] buffer = new byte[BufferSize];
            public List<byte> message = new List<byte>();
        }
        private void Receive(ReceiveState st)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("MockTcpServer.Receive: Wait for data from Socket connection");
                st.socket.BeginReceive(st.buffer, 0, ReceiveState.BufferSize, SocketFlags.None, (ar) => {
                    ReceiveState state = (ReceiveState)ar.AsyncState;
                    int received = state.socket.EndReceive(ar);
                    System.Diagnostics.Trace.WriteLine(string.Format("MockTcpServer.Receive: Read {0} from Socket connection", received));
                    if (received > 0)
                    {
                        state.message.AddRange(state.buffer.Take(received).ToList());
                        Receive(state); // read the remaining
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("MockTcpServer.Receive: Read message from Socket connection");
                        Received.Add(state.message.ToArray());
                        Receive(new ReceiveState(state.socket));
                    }
                }, st);
            }
            catch { }
        }
    }
}
