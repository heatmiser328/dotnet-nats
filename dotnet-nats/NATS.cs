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
        IDictionary<string, object> _opts;
        Dictionary<string, Subscription> _subscriptions;
        IServer _server;

        public NATS(IServerFactory factory, IDictionary<string,object> opts, ILog log)
        {
            _factory = factory;            
            _log = log;
            _subscriptions = new Dictionary<string, Subscription>();
            _opts = new Dictionary<string, object>();
            initOptions(opts);
            loadServers();
        }
        public NATS(IServerFactory factory, IDictionary<string, object> opts) : this(factory, opts, new log.ConsoleLog()) { }                
        #endregion

        #region Connect
        public static INATS Connect(IDictionary<string, object> opts, ILog log)
        {            
            INATS nats = new NATS(new ServerFactory(new TransportFactory(log), log), opts, log);
            nats.Connect();
            return nats;
        }
        public static INATS Connect(IDictionary<string, object> opts)
        {
            return NATS.Connect(opts, new log.ConsoleLog());
        }
        #endregion

        #region INATS
        public int Servers { get { return _servers != null ? _servers.Count : 0; } }
        public bool Connected { get { return _server != null && _server.Connected; } }

        public bool Connect()
        {
            if (_server == null)
                _server = NextServer();
            if (_server == null)
            {
                _log.Warn("Failed to retrieve a server from the queue");
                return false;
            }
            _server.Transport.Connected += (sender, b) =>
            {
                _log.Debug("Connected to server @ {0}", _server.URL);
                sendConnect();
            };
            _server.Transport.Disconnected += (sender, b) =>
            {

            };
            _server.Transport.Error += (sender, err) =>
            {

            };
            _server.Transport.ReceivedData += (sender, args) =>
            {

            };
            _server.Transport.Sent += (sender, sent) =>
            {

            };
            _log.Info("Connecting to Server @ {0}", _server.URL);
            _server.Transport.Open();            

            return true;
        }

        public void Close()
        {
            if (_server != null)
            {
                _log.Info("Disconnecting from Server @ {0}", _server.URL);
                _server.Transport.Close();
                _server = null;
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
            cmd.AppendFormat(@"""verbose"":{0}", this.verbose.ToString().ToLower());
            cmd.AppendFormat(@",""pedantic"":{0}", this.pedantic.ToString().ToLower());
            cmd.Append("}");
            //cmd.Append(System.Environment.NewLine);
            cmd.Append("\r\n");
            _server.Transport.Send(cmd.ToString());
        }
        #endregion

        #region options
        bool verbose { get { return getOption<bool>("verbose"); } }
        bool pedantic { get { return getOption<bool>("pedantic"); } }

        void initOptions(IDictionary<string, object> opts)
        {
            if (opts.ContainsKey("urls"))
            {
                _opts["uris"] = (string[])opts["urls"];
            }
            if (opts.ContainsKey("uris"))
            {
                _opts["uris"] = (string[])opts["uris"];
            }
            if (opts.ContainsKey("url"))
            {
                _opts["uris"] = new string[] { opts["url"].ToString() };
            }
            if (opts.ContainsKey("uri"))
            {
                _opts["uris"] = new string[] { opts["uri"].ToString() };
            }
        }
        T getOption<T>(string opt)
        {
            if (!_opts.ContainsKey(opt))
                return default(T);
            return (T)Convert.ChangeType(_opts[opt], typeof(T));
        }
        #endregion

        #region servers
        void loadServers()
        {            
            _servers = _factory.New(getOption<string[]>("uris"));
            _itr = _servers.GetEnumerator();
            _server = NextServer();
        }

        // need to expand this for clustering: basically, use a server until its maximum re-connect attempts is reached, then move to the next
        // eventually swing around to the first...
        IServer NextServer()
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
