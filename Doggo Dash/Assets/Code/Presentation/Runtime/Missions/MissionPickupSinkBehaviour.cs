using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Missions
{
    public sealed class MissionPickupSinkBehaviour : MonoBehaviour, IPickupCollectedSink
    {
        public MissionSystemBehaviour missionSystem = default!;

        private void Awake()
        {
            if (missionSystem == null)
            {
                Debug.LogError("[MissionPickupSink] missionSystem not assigned.");
                enabled = false;
            }
        }

        public void OnPickupCollected(PickupType type, int amount)
        {
            if (missionSystem == null) return;

            if (type == PickupType.Treat) missionSystem.ReportTreatCollected(amount);
            else if (type == PickupType.Gem) missionSystem.ReportGemCollected(amount);
        }
    }
}
