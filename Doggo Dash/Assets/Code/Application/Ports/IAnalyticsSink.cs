using System.Collections.Generic;

namespace Game.Application.Ports
{
    public interface IAnalyticsSink
    {
        void Track(string eventName, IReadOnlyDictionary<string, object> parameters);
    }
}
