using System.Collections.Generic;

namespace Game.Presentation.Runtime.CrashReporting
{
    internal sealed class CrashDedupeCache
    {
        private readonly int _capacity;
        private readonly float _ttlSeconds;
        private readonly Dictionary<int, float> _map = new();
        private readonly Queue<int> _order = new();

        public CrashDedupeCache(int capacity, float ttlSeconds)
        {
            _capacity = capacity < 8 ? 8 : capacity;
            _ttlSeconds = ttlSeconds <= 0f ? 2f : ttlSeconds;
        }

        public bool ShouldReport(int hash, float now)
        {
            Cleanup(now);

            if (_map.TryGetValue(hash, out var t))
            {
                if (now - t <= _ttlSeconds) return false;
                _map[hash] = now;
                return true;
            }

            _map[hash] = now;
            _order.Enqueue(hash);

            while (_order.Count > _capacity)
            {
                int old = _order.Dequeue();
                _map.Remove(old);
            }

            return true;
        }

        private void Cleanup(float now)
        {
            int iterations = _order.Count;
            for (int i = 0; i < iterations; i++)
            {
                int h = _order.Peek();
                if (_map.TryGetValue(h, out var t) && (now - t) > (_ttlSeconds * 4f))
                {
                    _order.Dequeue();
                    _map.Remove(h);
                }
                else break;
            }
        }
    }
}
