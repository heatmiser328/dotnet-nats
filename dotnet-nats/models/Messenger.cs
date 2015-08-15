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
		Queue<Message> _pending;
		class Incoming
		{
			public Incoming(byte[] data, int size)
			{
				Data = data;
				Size = size;
			}
			public byte[] Data {get;private set;}
			public int Size {get;private set;}
		}
		ConcurrentQueue<Incoming> _incoming;
		private volatile bool _processing;

        public Messenger(ILog log)
        {            
            _log = log;
            _receiveStatus = ReceiveStatus.AWAITING_CONTROL;
            _buffer = new List<byte>();
			_pending = new Queue<Message>();
			_incoming = new ConcurrentQueue<Incoming>();
			_processing = false;
        }

		public event EventHandler<Message> Msg;
		public event EventHandler Ping;
		public event EventHandler Pong;
		public event EventHandler Info;
		public event EventHandler Error;

        public void Receive(byte[] data, int size)
        {
			_incoming.Enqueue(new Incoming(data, size));
			if (_processing) return;
			
            //Task.Factory.StartNew(() =>
            //{
                try
                {
					_processing = true;
					Incoming incoming;
					while (_incoming.TryDequeue(out incoming))
					{
						processIncoming(incoming.Data, incoming.Size);
					}	
					
                }
                catch (Exception ex)
                {
                    _log.Error("Failed to process received message", ex);
                }
				finally
				{
					_processing = false;
				}
            //});			
        }

        string ControlStatus
        {
            get
            {
                if (_receiveStatus == ReceiveStatus.AWAITING_CONTROL)
                    return "AWAITING_CONTROL";
                if (_receiveStatus == ReceiveStatus.AWAITING_PAYLOAD)
                    return "AWAITING_PAYLOAD";
                return "WTH?";
            }
        }

		StringBuilder buffer = new StringBuilder();
		void processIncoming(byte[] cldata, int clsize)
		{
            _log.Debug("++ " + clsize.ToString() + " " + Encoding.UTF8.GetString(cldata, 0, clsize).Replace(System.Environment.NewLine, "RN"));                        
            _buffer.AddRange(cldata.Take(clsize));                        
			while (hasData())
			{
				_log.Debug("** " + _buffer.Count.ToString() + " " + Encoding.UTF8.GetString(_buffer.ToArray(), 0, _buffer.Count).Replace(System.Environment.NewLine, "RN"));
                _log.Debug(ControlStatus);
                if (_receiveStatus == ReceiveStatus.AWAITING_CONTROL)
				{		                    								
                    string ctl = nextControl();
                    if (ctl != null)
                    {
                        _log.Debug("OP: {0}", ctl);
                        if (ctl.StartsWith(Message.MSG))
                        {
							Message msg = Message.Parse(ctl);
                            _pending.Enqueue(msg);
                            _log.Debug("MSG {0} : {1}", msg.Size, _buffer.Count);
                            _receiveStatus = ReceiveStatus.AWAITING_PAYLOAD;
                            if (_buffer.Count < msg.Size + Message.CRLF.Length)
							{
								_log.Debug("Wait for remainder of message");
                                return;
							}
                        }
                        else if (ctl.StartsWith(Message.PING))
                        {
                            _log.Debug("PING");
                            RaisePing();
                        }
                        else if (ctl.StartsWith(Message.PONG))
                        {
                            _log.Debug("PONG");
                            RaisePong();
                        }
                        else if (ctl.StartsWith(Message.OK))
                        {
                            _log.Debug("OK");
                        }
                        else if (ctl.StartsWith(Message.ERR))
                        {
                            _log.Debug("Error");
                            RaiseError();
                        }
                        else if (ctl.StartsWith(Message.INFO))
                        {
                            _log.Debug("Info");
                            RaiseInfo();
                        }
                        else if (ctl.StartsWith(Message.CRLF))
                        {
                            _log.Debug("CRLF");                                        
                        }
                        else
                        {
                            _log.Warn("Unknown control received: {0}", ctl);
                        }
					}
				}
                else if (_receiveStatus == ReceiveStatus.AWAITING_PAYLOAD)
				{
                    Message msg = _pending.Peek();
					if (_buffer.Count < msg.Size)// + Message.CRLF.Length)
					{
						_log.Debug("Still waiting for remainder of message");
                        return;
					}
					_log.Debug("Reading Message {0}", msg.Size);// + Message.CRLF.Length);								
                    msg.Data = readData(msg.Size);// + Message.CRLF.Length);
					int idx = msg.Data.LastIndexOf(Message.CRLF);
					if (idx > -1)
						msg.Data = msg.Data.Remove(idx, Message.CRLF.Length);
                    _pending.Dequeue();
                    RaiseMessage(msg);
                    _receiveStatus = ReceiveStatus.AWAITING_CONTROL;
				}
            }
		}
		
		bool hasData()
		{
			return _buffer.Count > 0;
		}
		
		string readData(int size)
		{
            size = Math.Min(size, _buffer.Count);
			string data = Encoding.UTF8.GetString(_buffer.Take(size).ToArray());
			_buffer.RemoveRange(0, size);
			return data;
		}
        string nextControl()
        {
            /*
            int pos = _buffer.FindIndex(b => { return b == '\n'; });
            if (pos >= 0)
            {
                string msg = Encoding.UTF8.GetString(_buffer.Take(pos + 1).ToArray());
                _buffer.RemoveRange(0, pos + 1);
                return msg;
            }
            */
            for (int i = 0; i < _buffer.Count; i++)
            {
                if (i > 0 && _buffer[i] == '\n' && _buffer[i - 1] == '\r')
                {
					return readData(i+1);
                }
            }

            return null;
        }
        bool nextIsControl()
        {
            string ctl = Encoding.UTF8.GetString(_buffer.Take(4).ToArray());
            return ctl.StartsWith(Message.MSG) || 
				ctl.StartsWith(Message.PING) ||
				ctl.StartsWith(Message.PONG) ||
				ctl.StartsWith(Message.OK)	 ||
				ctl.StartsWith(Message.ERR)	 ||
				ctl.StartsWith(Message.INFO);
        }
				
		string nextMessage(int size)
		{
			size = Math.Min(size, _buffer.Count);
			return readData(size);
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
