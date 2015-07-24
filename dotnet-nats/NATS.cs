using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class NATS : INATS, IDisposable
    {
        IList<ITransport> _clients;

        public NATS(string url)
        {

        }

        #region INATS
        public bool Connect()
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void Subscribe(string topic, Action handler)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(string topic)
        {
            throw new NotImplementedException();
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
