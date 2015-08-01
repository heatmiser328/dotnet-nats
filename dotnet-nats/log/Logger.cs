using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats.log
{
    public class Logger : ILog
    {
        const string CRLF = "\r\n";
        const string CRLF_DISPLAY = //"<cr><lf>";	
                                    "\\r\\n";
        IOutput _output;

        public Logger(IOutput output, string level)
        {
			_output = output;
            Level = level;
        }
        public Logger(IOutput output) : this(output, "info"){}
        public Logger() : this(new ConsoleOutput()){}

        #region ILog Members
        public string Level { get; set; }

        public void Log(string level, string msg, Exception ex = null, params object[] args)
        {
            if (IsDebug(level))
                Debug(msg, args);
            else if (IsInfo(level))
                Info(msg, args);
            else if (IsWarn(level))
                Warn(msg, args);
            else if (IsError(level))
                Error(msg, ex, args);
            else if (IsFatal(level))
                Fatal(msg, args);
            else
                Trace(msg, args);
        }

        public void Trace(string msg, params object[] args)
        {
            Write("TRACE", msg, args);
        }

        public void Debug(string msg, params object[] args)
        {
            Write("DEBUG", msg, args);
        }

        public void Info(string msg, params object[] args)
        {
            Write("INFO", msg, args);
        }

        public void Warn(string msg, params object[] args)
        {
            Write("WARN", msg, args);
        }

        public void Error(string msg, params object[] args)
        {
            Write("ERROR", msg, args);
        }

        public void Error(string msg, Exception ex, params object[] args)
        {
            string em = ExceptionMessage(ex);
            if (!string.IsNullOrEmpty(em))
                msg += System.Environment.NewLine + em;
            Write("ERROR", msg, args);
        }

        public void Fatal(string msg, params object[] args)
        {
            Write("FATAL", msg, args);
        }

        #endregion
		
		bool IsTrace(string level)
		{
        	return level.Equals("trace", StringComparison.InvariantCultureIgnoreCase);
		}
		bool IsDebug(string level)
		{
        	return level.Equals("debug", StringComparison.InvariantCultureIgnoreCase);
		}
		bool IsInfo(string level)
		{
			return level.Equals("info", StringComparison.InvariantCultureIgnoreCase);
		}
		bool IsWarn(string level)
		{
			return level.Equals("warn", StringComparison.InvariantCultureIgnoreCase);
		}
		bool IsError(string level)
		{
			return level.Equals("error", StringComparison.InvariantCultureIgnoreCase);
		}
		bool IsFatal(string level)
		{
			return level.Equals("fatal", StringComparison.InvariantCultureIgnoreCase);
		}
		int logLevel(string level)
		{
			if (IsTrace(level)) return 0;
			if (IsDebug(level)) return 1;
			if (IsInfo(level)) return 2;
			if (IsWarn(level)) return 3;
			if (IsError(level)) return 4;
			if (IsFatal(level)) return 5;
			return 6;
		}
        bool DisplayLevel(string level)
        {
			return logLevel(level) >= logLevel(this.Level);
        }

        string ExceptionMessage(Exception ex)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Exception e = ex;
            while (e != null)
            {
                sb.AppendLine(e.Message);
                e = e.InnerException;
            }
            return sb.ToString();
        }

        void Write(string level, string msg, params object[] args)
        {
            if (DisplayLevel(level))
            {
                msg = args != null ? string.Format(msg, args) : msg;
                if (msg.EndsWith(CRLF))
                    msg = msg.Remove(msg.LastIndexOf(CRLF), CRLF.Length) + CRLF_DISPLAY;
				_output.Out(level, msg);
            }                                
        }
    }
}
