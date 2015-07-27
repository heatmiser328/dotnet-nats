using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Shouldly;
using NSubstitute;

namespace tests
{
    public interface IEngine
    {
        event EventHandler Idling;
        event EventHandler<LowFuelWarningEventArgs> LowFuelWarning;
        event EventHandler<EventArgs<bool>> Started;
        event Action<int> RevvedAt;
    }

    public class LowFuelWarningEventArgs : EventArgs
    {
        public int PercentLeft { get; private set; }
        public LowFuelWarningEventArgs(int percentLeft)
        {
            PercentLeft = percentLeft;
        }
    }

    public class BooleanEventArgs : EventArgs
    {
        public BooleanEventArgs(bool b)
        {
            Value = b;
        }
        public bool Value { get; private set; }
    }

    public class EventArgs<T> : EventArgs
    {
        public EventArgs(T v)
        {
            Value = v;
        }
        public T Value { get; private set; }
    }


    public class EngineTest
    {
        [Fact]
        public void events()
        {
            IEngine engine = Substitute.For<IEngine>();
            var numberOfEvents = 0;

            engine.LowFuelWarning += (sender, args) => numberOfEvents++;
            engine.Started += (sender, args) => numberOfEvents++;

            //Raise event with specific args, any sender:
            engine.LowFuelWarning += Raise.EventWith(new LowFuelWarningEventArgs(10));
            //Raise event with specific args and sender:
            engine.LowFuelWarning += Raise.EventWith(new object(), new LowFuelWarningEventArgs(10));

            engine.Started += Raise.EventWith(new EventArgs<bool>(false));

            numberOfEvents.ShouldBe(3);
        }
    }
}
