using UnityEngine;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Missions
{
    public sealed class MissionRewardsBehaviour : MonoBehaviour
    {
        public MissionSystemBehaviour missionSystem = default!;
        public RunRewardTrackerBehaviour runRewards = default!;

        private void Awake()
        {
            if (missionSystem == null || runRewards == null)
            {
                Debug.LogError("[MissionRewards] missing refs.");
                enabled = false;
                return;
            }

            missionSystem.Manager.OnMissionCompleted += OnCompleted;
        }

        private void OnDestroy()
        {
            if (missionSystem != null)
                missionSystem.Manager.OnMissionCompleted -= OnCompleted;
        }

        private void OnCompleted(Game.Application.Services.MissionManager.ActiveMission m)
        {
            // Requires RunRewardTrackerBehaviour to have AddKibble/AddGems.
            if (m.Definition.rewardKibble > 0) runRewards.AddKibble(m.Definition.rewardKibble);
            if (m.Definition.rewardGems > 0) runRewards.AddGems(m.Definition.rewardGems);
        }
    }
}
