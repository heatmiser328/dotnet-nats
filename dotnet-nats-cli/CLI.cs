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
            INATS nats = null;
            try
            {
                ILog log = new dotnet_nats.log.ConsoleLog();
                ITransportFactory tf = new TransportFactory(log);
                IServerFactory sf = new ServerFactory(tf, log);
                nats = new NATS(sf, opts, log);
                if (nats.Connect())
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
                        Console.Out.WriteLine("Unknow mode supplied: {0}", opts.mode);
                    }
                }
            }
            catch
            {
                throw;
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
                log.Debug("NATS Client: Sending {0}", i);
                nats.Publish(subject,msg);
            }
        }

        void subscribe(INATS nats, string subject, int count, ILog log)
        {
            nats.Subscribe(subject, (data) =>
            {
                log.Debug("Received: {0}", data);
            });
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
                .Required();

            p.Setup(arg => arg.loglevel)
                .As('l', "loglevel")
                .WithDescription("Log Level")
                .SetDefault("WARN");
            p.Setup(arg => arg.mode)
                .As('m', "mode")
                .WithDescription("Mode (PUB|SUB)")
                .SetDefault("PUB");
            p.Setup(arg => arg.subject)
                .As('s', "subject")
                .WithDescription("Subject")
                .Required();
            p.Setup(arg => arg.data)
                .As('d', "data")
                .WithDescription("Data or path to file containing data to publish");
            p.Setup(arg => arg.count)
                .As('c', "count")
                .WithDescription("Number of items to publish/receive")
                .SetDefault(10);

            p.SetupHelp("?", "h", "help")
                .Callback(text => Console.Out.WriteLine(text));

            if (p.Parse(args).HasErrors)
            {
                p.HelpOption.ShowHelp(p.Options);
                throw new Exception("Failed to parse command line arguments");
            }
                
            return p.Object;
        }
    }
}
