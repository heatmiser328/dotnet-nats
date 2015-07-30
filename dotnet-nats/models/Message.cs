using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public class Message
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
		
		public string Subject {get; set;}
		public int SubscriptionID {get; set;}
		public string ReplyTo {get; set;}
		public int Size {get; set;}
		public string Data {get; set;}

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

        public static string Pong()
        {
            return PONG + CRLF;
        }

        public static Message Parse(string s)
        {
            const string cPattern = @"^MSG\s+([^\s\r\n]+)\s+([^\s\r\n]+)\s+(([^\s\r\n]+)[^\S\r\n]+)?(\d+)\r\n";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(cPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var matches = regex.Matches(s);
            if (matches == null || matches.Count < 1)
                return null;
            var groups = matches[0].Groups;
            var msg = new Message();
            msg.Subject = groups.Count > 1 ? groups[1].Value : string.Empty;
            msg.SubscriptionID = groups.Count > 2 ? Int32.Parse(groups[2].Value) : -1;
            msg.ReplyTo = groups.Count > 4 ? groups[4].Value : string.Empty;
            msg.Size = groups.Count > 5 ? Int32.Parse(groups[5].Value) : 0;

            return msg;
        }

    }
}
