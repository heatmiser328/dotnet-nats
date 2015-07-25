using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public interface INATS
    {
        bool Connect();        
        void Close();
        void Subscribe(string topic, Action handler);
        void Unsubscribe(string topic);
        void Publish(string topic, string data);
        void Publish(string topic, byte[] data);
        void Flush();
    }
}
