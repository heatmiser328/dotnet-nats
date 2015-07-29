using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class Messenger : IMessenger
    {
        enum ReceiveStatus { AWAITING_CONTROL = 0, AWAITING_PAYLOAD };        
        ILog _log;
        ConcurrentQueue<Action<string>> _pongs;
        ReceiveStatus _receiveStatus;
        List<byte> _buffer;
        object _lock;

        public Messenger(ILog log)
        {            
            _log = log;
            _pongs = new ConcurrentQueue<Action<string>>();
            _receiveStatus = ReceiveStatus.AWAITING_CONTROL;
            _buffer = new List<byte>();
            _lock = new object();
        }

        public void Ping(Action<string> handler)
        {
            _pongs.Enqueue(handler);
        }

        public void Receive(byte[] data, int size)
        {
            byte[] cldata = data;
            int clsize = size;
            //Task.Factory.StartNew(() =>
            //{
                try
                {
                    lock (_lock)
                    {
                        _buffer.AddRange(data.Take(clsize));
                        if (_receiveStatus == ReceiveStatus.AWAITING_CONTROL)
                        {
                            // loop over buffer, retrieing each "operation" from it
                            string op = null;
                            while ((op = nextOp(_buffer)) != null)
                            {
                                _log.Trace("OP: {0}", op);
                                if (op.StartsWith(Message.MSG))
                                {
                                    _log.Debug("Message");
                                }
                                else if (op.StartsWith(Message.PING))
                                {
                                    _log.Debug("PING");
                                }
                                else if (op.StartsWith(Message.PONG))
                                {
                                    _log.Debug("PONG");
                                    Action<string> pong;                                    
                                    if (_pongs.TryDequeue(out pong))
                                    {
                                        pong(op);
                                    }                                    
                                }
                                else if (op.StartsWith(Message.OK))
                                {
                                    _log.Debug("OK");
                                }
                                else if (op.StartsWith(Message.ERR))
                                {
                                    _log.Debug("Error");
                                }
                                else if (op.StartsWith(Message.INFO))
                                {
                                    _log.Debug("Info");
                                }
                                else
                                {
                                    _log.Warn("Unkown control received: {0}", op);
                                }
                            }
                        }
                        else if (_receiveStatus == ReceiveStatus.AWAITING_PAYLOAD)
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to process received message", ex);
                }
            //});
        }

        string nextOp(List<byte> buffer)
        {
            /*
            int pos = buffer.FindIndex(b => { return b == '\n'; });
            if (pos >= 0)
            {
                string msg = Encoding.UTF8.GetString(buffer.Take(pos + 1).ToArray());
                buffer.RemoveRange(0, pos + 1);
                return msg;
            }
            */
            for (int i = 0; i < buffer.Count; i++)
            {
                if (i > 0 && buffer[i] == '\n' && buffer[i - 1] == '\r')
                {
                    string msg = Encoding.UTF8.GetString(buffer.Take(i + 1).ToArray());
                    buffer.RemoveRange(0, i + 1);
                    return msg;
                }
            }

            return null;
        }
    }

}
