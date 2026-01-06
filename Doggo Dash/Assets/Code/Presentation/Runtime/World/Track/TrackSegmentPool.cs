using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Runtime.World.Track
{
    public sealed class TrackSegmentPool
    {
        private readonly Dictionary<TrackSegmentBehaviour, Stack<TrackSegmentBehaviour>> _pool = new();

        public TrackSegmentBehaviour Rent(TrackSegmentBehaviour prefab, Transform parent)
        {
            if (_pool.TryGetValue(prefab, out var stack) && stack.Count > 0)
            {
                var inst = stack.Pop();
                inst.gameObject.SetActive(true);
                inst.transform.SetParent(parent, worldPositionStays: true);
                return inst;
            }

            return Object.Instantiate(prefab, parent);
        }

        public void Return(TrackSegmentBehaviour prefabKey, TrackSegmentBehaviour instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(null, worldPositionStays: true);

            if (!_pool.TryGetValue(prefabKey, out var stack))
            {
                stack = new Stack<TrackSegmentBehaviour>();
                _pool[prefabKey] = stack;
            }

            stack.Push(instance);
        }
    }
}
