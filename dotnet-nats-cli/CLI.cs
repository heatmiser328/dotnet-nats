using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dotnet_nats;
using Fclp;

namespace dotnet_nats_cli
{
    internal class CLI
    {        
        class CLIOptions : Options
        {
            public string loglevel { get; set; }
            public string mode { get; set; }
            public string subject { get; set; }
            public string data { get; set; }
            public int count { get; set; }
        }        

        public CLI()
        {            
        }

        public void Run(string[] args)
        {
            run(parse(args));
        }

        void run(CLIOptions opts)
        {
            ILog log = null;
            INATS nats = null;
            try
            {
                log = new dotnet_nats.log.Logger(opts.loglevel);
                IFactory f = new Factory(log);
                nats = new NATS(f, opts, log);
                var t = nats.Connect();
                t.Wait();
                if (t.Result) 
                {                    
                    if (opts.mode.Equals("pub", StringComparison.InvariantCultureIgnoreCase))
                    {
                        publish(nats, opts.subject, opts.data, opts.count, log);
                    }
                    else if (opts.mode.Equals("sub", StringComparison.InvariantCultureIgnoreCase))
                    {
                        subscribe(nats, opts.subject, opts.count, log);
                    }
                    else
                    {
                        log.Fatal("Unknown mode supplied: {0}", opts.mode);                        
                    }
                }
                else
                {
                    throw new Exception("Failed to connect to server");
                }
            }
            catch (Exception ex)
            {
                if (log != null)
                    log.Error("Error processing", ex);
                //throw;
            }
            finally
            {
                if (nats != null)
                    nats.Close();
                nats = null;
            }
        }

        void publish(INATS nats, string subject, string data, int count, ILog log)
        {
            if (System.IO.File.Exists(data))
            {
                data = System.IO.File.ReadAllText(data);
            }
            for (int i = 0; i < count && nats.Connected; i++)
            {
                string msg = string.Format(data, i);
                log.Info("Sending {0}", i);
                nats.Publish(subject,msg);
                //System.Threading.Thread.Sleep(100);//System.Threading.Timeout.Infinite);
            }
        }

        void subscribe(INATS nats, string subject, int count, ILog log)
        {
            nats.Subscribe(subject, (data) =>
            {
                log.Info("Received {0}", count);
                //log.Trace(data);
                count--;
            });
            while (count > 0) { System.Threading.Thread.Sleep(250); }
        }

        CLIOptions parse(string[] args)
        {
            var p = new FluentCommandLineParser<CLIOptions>();

            p.Setup(arg => arg.verbose)
                .As('v', "verbose")
                .WithDescription("Verbose protocol")
                .SetDefault(false);
            p.Setup(arg => arg.pedantic)
                .As('p', "pedantic")
                .WithDescription("Pedantic subjects")
                .SetDefault(false);
            p.Setup(arg => arg.reconnectDelay)
                .As('r', "reconnectDelay")
                .WithDescription("Delay between reconnects (milliseconds)")
                .SetDefault(500);
            p.Setup(arg => arg.uris)
                .As('u', "uris")
                .WithDescription("List of server URIs")
                .SetDefault(new List<string> {"nats://localhost:4222"});
                //.Required();

            p.Setup(arg => arg.loglevel)
                .As('l', "loglevel")
                .WithDescription("Log Level")
                .SetDefault("WARN");
            p.Setup(arg => arg.mode)
                .As('m', "mode")
                .WithDescription("Mode (PUB|SUB)")
                .Required();                
            p.Setup(arg => arg.subject)
                .As('s', "subject")
                .WithDescription("Subject")
                .Required();
            p.Setup(arg => arg.data)
                .As('d', "data")
                .WithDescription("Data or path to file containing data to publish");
                //.Required();
            p.Setup(arg => arg.count)
                .As('c', "count")
                .WithDescription("Number of items to publish/receive")
                .SetDefault(10);

            p.SetupHelp("?", "h", "help")
                .Callback(text => Console.Out.WriteLine(text));

            var result = p.Parse(args);
            if (result.HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                throw new Exception(result.ErrorText);
            }
                
            return p.Object;
        }
    }
}
