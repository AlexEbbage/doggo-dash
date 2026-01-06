using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Run
{
    public sealed class PickupEventRouterBehaviour : MonoBehaviour, IPickupCollectedSink
    {
        public MonoBehaviour[] sinks = default!;

        public void OnPickupCollected(PickupType type, int amount)
        {
            if (sinks == null) return;

            for (int i = 0; i < sinks.Length; i++)
            {
                if (sinks[i] is IPickupCollectedSink sink)
                    sink.OnPickupCollected(type, amount);
            }
        }
    }
}
