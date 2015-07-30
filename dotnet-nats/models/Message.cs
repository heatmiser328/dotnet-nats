using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public static class Message
    {
        public const string CONNECT = "CONNECT";
        public const string PUB = "PUB";
        public const string SUB = "SUB";
        public const string UNSUB = "UNSUB";
        public const string MSG = "MSG";
        public const string PONG = "PONG";
        public const string PING = "PING";
        public const string INFO = "INFO";
        public const string ERR = "-ERR";
        public const string OK = "+OK";
        public const string CRLF = "\r\n";

        public static string Connect(Options opts)
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append(CONNECT);
            cmd.Append(" {");
            cmd.AppendFormat(@"""verbose"":{0}", opts.verbose.ToString().ToLower());
            cmd.AppendFormat(@",""pedantic"":{0}", opts.pedantic.ToString().ToLower());
            cmd.Append("}");
            //cmd.Append(System.Environment.NewLine);
            cmd.Append(CRLF);
            return cmd.ToString();
        }

        public static string Publish(string subject, string data)
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append(PUB);
            cmd.AppendFormat(" {0} {1}", subject, data.Length);
            cmd.Append(CRLF);
            cmd.AppendFormat(data);
            cmd.Append(CRLF);
            return cmd.ToString();
        }

        public static string Subscribe(int sid, string subject, string queue = " ")
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append(SUB);
            cmd.AppendFormat(" {0} {1} {2}", subject, queue, sid);
            cmd.Append(CRLF);
            return cmd.ToString();
        }

        public static string Unsubscribe(int sid)
        {
            StringBuilder cmd = new StringBuilder();
            cmd.Append(UNSUB);
            cmd.AppendFormat(" {0} 0", sid);
            cmd.Append(CRLF);
            return cmd.ToString();
        }

        public static string Ping()
        {
            return PING + CRLF;
        }
    }
}
