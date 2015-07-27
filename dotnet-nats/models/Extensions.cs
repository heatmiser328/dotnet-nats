using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnet_nats
{
    public static class ActionExtensions
    {
        public static async void ExecuteAfter(this Action action, int milliseconds)
        {
            if (milliseconds > 0)
                await Task.Delay(milliseconds);
            action();
        }
    }
}
