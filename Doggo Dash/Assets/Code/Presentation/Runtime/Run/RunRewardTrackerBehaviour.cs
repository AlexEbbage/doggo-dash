using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunRewardTrackerBehaviour : MonoBehaviour, IPickupCollectedSink
    {
        public int Kibble { get; private set; }
        public int Gems { get; private set; }
        public int BadFoodHits { get; private set; }

        public void OnPickupCollected(PickupType type, int amount)
        {
            switch (type)
            {
                case PickupType.Treat:
                    Kibble += Mathf.Max(0, amount);
                    break;
                case PickupType.Gem:
                    Gems += Mathf.Max(0, amount);
                    break;
                case PickupType.BadFood:
                    BadFoodHits += 1;
                    break;
                default:
                    break;
            }
        }

        public void ResetRun()
        {
            Kibble = 0;
            Gems = 0;
            BadFoodHits = 0;
        }
    }
}
