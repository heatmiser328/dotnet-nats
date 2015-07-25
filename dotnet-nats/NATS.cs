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
        ILog _log;
        Queue<Server> _servers;
        Dictionary<string, Action<string>> _subscriptions;
        Server _server;

        public NATS(string[] urls, ILog log)
        {
            _log = log;
            _servers = new Queue<Server>();
            _subscriptions = new Dictionary<string, Action<string>>();
            foreach (string url in urls)
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    _log.Warn("Unrecognized server url: {0}. Skipping...", url);
                    continue;
                }
                _servers.Enqueue(new Server(url, _log));
            }                
        }
        public NATS(string[] urls) : this(urls, new log.ConsoleLog()) {}
        public NATS(string url, ILog log) : this(new string[] {url}, log) {}
        public NATS(string url) : this(new string[] { url }) {}

        #region INATS
        public bool Connect()
        {
            _server = _servers.Dequeue();
            if (_server == null)
            {
                _log.Warn("Failed to retrieve a server from the queue");
                return false;
            }
            _server.Transport.Connected += (sender, b) =>
            {

            };
            _server.Transport.Disconnected += (sender, b) =>
            {

            };
            _server.Transport.Error += (sender, err) =>
            {

            };
            _server.Transport.Received += (sender, args) =>
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

        public void Subscribe(string topic, Action<string> handler)
        {
            _subscriptions[topic] = handler;
            // send to server
        }

        public void Unsubscribe(string topic)
        {
            if (_subscriptions.ContainsKey(topic))
                _subscriptions.Remove(topic);
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

        #region IDisposable
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
