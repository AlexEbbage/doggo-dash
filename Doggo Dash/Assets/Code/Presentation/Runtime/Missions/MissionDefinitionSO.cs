using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Missions
{
    [CreateAssetMenu(menuName = "Game/Missions/Mission Definition", fileName = "MissionDefinition")]
    public sealed class MissionDefinitionSO : ScriptableObject
    {
        [Header("Mission")]
        public MissionType type;
        [Min(1)] public int target = 10;

        [Header("Rewards")]
        [Min(0)] public int rewardKibble = 50;
        [Min(0)] public int rewardGems = 0;

        [Header("Selection")]
        [Min(1)] public int weight = 1;

        [Header("UI")]
        public string displayName = "Mission";
    }
}
