using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.World.Obstacles
{
    [CreateAssetMenu(menuName = "Game/Configs/Obstacle Pattern", fileName = "ObstaclePattern")]
    public sealed class ObstaclePatternSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Row
        {
            public float zOffset = 8f;
            public ObstacleLaneMask lanes = ObstacleLaneMask.Middle;
            public string obstacleId = "RockFull";
            [Range(0f, 1f)] public float chance = 1f;
        }

        public Row[] rows = default!;
    }
}
