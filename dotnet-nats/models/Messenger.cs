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
        ReceiveStatus _receiveStatus;
        List<byte> _buffer;
        object _lock;
		ConcurrentQueue<Message> _pending;

        public Messenger(ILog log)
        {            
            _log = log;
            _receiveStatus = ReceiveStatus.AWAITING_CONTROL;
            _buffer = new List<byte>();
            _lock = new object();
			_pending = new ConcurrentQueue<Message>();
        }

		public event EventHandler<Message> Msg;
		public event EventHandler Ping;
		public event EventHandler Pong;
		public event EventHandler Info;
		public event EventHandler Error;
		
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
                        // loop over buffer, retrieving each "operation" from it
                        string op = null;
                        while ((op = nextOp(_buffer)) != null)
                        {
                            if (_receiveStatus == ReceiveStatus.AWAITING_CONTROL)
                            {
                                _log.Trace("OP: {0}", op);
                                if (op.StartsWith(Message.MSG))
                                {
                                    _log.Debug("MSG");
                                    _pending.Enqueue(Message.Parse(op));
                                    _receiveStatus = ReceiveStatus.AWAITING_PAYLOAD;
                                }
                                else if (op.StartsWith(Message.PING))
                                {
                                    _log.Debug("PING");
                                    RaisePing();
                                }
                                else if (op.StartsWith(Message.PONG))
                                {
                                    _log.Debug("PONG");
                                    RaisePong();
                                }
                                else if (op.StartsWith(Message.OK))
                                {
                                    _log.Debug("OK");
                                }
                                else if (op.StartsWith(Message.ERR))
                                {
                                    _log.Debug("Error");
                                    RaiseError();
                                }
                                else if (op.StartsWith(Message.INFO))
                                {
                                    _log.Debug("Info");
                                    RaiseInfo();
                                }
                                else
                                {
                                    _log.Warn("Unknown control received: {0}", op);
                                }
                            }
                            else if (_receiveStatus == ReceiveStatus.AWAITING_PAYLOAD)
                            {
                                _log.Debug(op);
                                Message msg;
                                if (_pending.TryDequeue(out msg))
                                {
                                    msg.Data = op.Remove(op.LastIndexOf(Message.CRLF), Message.CRLF.Length);
                                    RaiseMessage(msg);
                                }
                                _receiveStatus = ReceiveStatus.AWAITING_CONTROL;
                            }
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
				
		#region events
		void RaiseMessage(Message msg)
		{
			if (Msg != null)
                Msg(this, msg);
		}
		void RaisePing()
		{
			if (Ping != null)
				Ping(this, EventArgs.Empty);
		}
		void RaisePong()
		{
			if (Pong != null)
				Pong(this, EventArgs.Empty);
		}
		void RaiseInfo()
		{
			if (Info != null)
				Info(this, EventArgs.Empty);
		}
		void RaiseError()
		{
			if (Error != null)
				Error(this, EventArgs.Empty);
		}
		#endregion
    }

}
