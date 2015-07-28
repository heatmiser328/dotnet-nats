using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_sockets;

namespace dotnet_nats
{
    public class NATS : INATS, IDisposable
    {
        #region Constructors
        IServerFactory _factory;
        ILog _log;
        ICollection<IServer> _servers;
        IEnumerator<IServer> _itr;
        Options _opts;
        IDictionary<string, Subscription> _subscriptions;
        IServer _server;
        bool _closing;

        public NATS(IServerFactory factory, Options opts, ILog log)
        {
            _factory = factory;            
            _log = log;
            _subscriptions = new Dictionary<string, Subscription>();
            _opts = opts;            
            loadServers();
        }
        public NATS(IServerFactory factory, Options opts) : this(factory, opts, new log.ConsoleLog()) { }                
        #endregion

        #region Connect
        public static INATS Connect(Options opts, ILog log)
        {            
            INATS nats = new NATS(new ServerFactory(new TransportFactory(log), log), opts, log);
            nats.Connect();
            return nats;
        }
        public static INATS Connect(Options opts)
        {
            return NATS.Connect(opts, new log.ConsoleLog());
        }
        #endregion

        #region INATS
        public int Servers { get { return _servers != null ? _servers.Count : 0; } }
        public bool Connected { get { return _server != null && _server.Connected; } }

        public bool Connect()
        {
            try
            {
                if (_server == null)
                    _server = nextServer();
                if (_server == null)
                {
                    _log.Warn("Failed to retrieve a server from the queue");
                    return false;
                }
                _log.Info("Connecting to Server @ {0}", _server.URL);
                _server.Transport.Open();

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to connect to server", ex);
                throw;
            }
        }

        public void Close()
        {
            try
            {            
                if (_server != null)
                {
                    _log.Info("Disconnecting from Server @ {0}", _server.URL);
                    _closing = true;
                    _server.Transport.Close();
                }            
            }
            catch (Exception ex)
            {
                _log.Error("Failed to connect to server", ex);
                throw;
            }
            finally
            {
                _closing = false;
                _server = null;
                _itr.Reset();
            }
        }

        public void Subscribe(string subject, Action<string> handler)
        {
            _subscriptions[subject] = new Subscription(subject, handler);
            // send to server
        }

        public void Unsubscribe(string subject)
        {
            if (_subscriptions.ContainsKey(subject))
                _subscriptions.Remove(subject);
            // send to server
        }

        public void Publish(string topic, string data)
        {
            throw new NotImplementedException();
        }

        public void Publish(string topic, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region send
        void sendConnect()
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append("CONNECT {");
            cmd.AppendFormat(@"""verbose"":{0}", _opts.verbose.ToString().ToLower());
            cmd.AppendFormat(@",""pedantic"":{0}", _opts.pedantic.ToString().ToLower());
            cmd.Append("}");
            //cmd.Append(System.Environment.NewLine);
            cmd.Append("\r\n");
            _server.Transport.Send(cmd.ToString());
        }
        #endregion

        #region servers
        void loadServers()
        {            
            _servers = _factory.New(_opts.uris.ToArray());
            _servers.ToList().ForEach(connectServer);            
            _itr = _servers.GetEnumerator();            
            _server = nextServer();
        }

        void connectServer(IServer s)
        {
            s.Transport.Connected += (sender, b) =>
            {
                _log.Debug("Connected to server @ {0}", s.URL);
                sendConnect();
            };
            s.Transport.Disconnected += (sender, b) =>
            {
                if (_closing) return;
                _log.Warn("Disconnected from server @ {0}. Reconnecting...", s.URL);                
                new Action(() => { Connect(); }).ExecuteAfter(_opts.reconnectDelay);
            };
            s.Transport.Error += (sender, err) =>
            {
                _log.Error("Error with server @ {0}", s.URL, err.Value);
            };
            s.Transport.ReceivedData += (sender, args) =>
            {

            };
            s.Transport.Sent += (sender, sent) =>
            {
                _log.Trace("Sent {0} bytes to server @ {1}", sent.Value, s.URL);
            };
        }

        // need to expand this for clustering: basically, use a server until its maximum re-connect attempts is reached, then move to the next
        // eventually swing around to the first...
        IServer nextServer()
        {
            if (!_itr.MoveNext())
            {
                _itr.Reset();
                _itr.MoveNext();
            }
            return _itr.Current;            
        }

        #endregion

        #region IDisposable
        public void Dispose()
        {
            //throw new NotImplementedException();
        }
        #endregion
    }
}
