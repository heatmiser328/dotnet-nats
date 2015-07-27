using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new CLI().Run(args);
            }
            catch(Exception ex)
            {
                while (ex != null)
                {
                    Console.Error.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }                    
            }            
        }
    }
}
