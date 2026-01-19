using UnityEngine;
using Game.Domain.ValueObjects;
using System;

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

        [SerializeField, HideInInspector]
        private ObstaclePatternSO[] packPatterns = Array.Empty<ObstaclePatternSO>();

        public Row[] GetRows(System.Random rng = null)
        {
            if (packPatterns != null && packPatterns.Length > 0)
            {
                int idx = rng != null ? rng.Next(0, packPatterns.Length) : UnityEngine.Random.Range(0, packPatterns.Length);
                var chosen = packPatterns[idx];
                if (chosen != null && chosen.rows != null) return chosen.rows;
            }

            return rows;
        }

        public void SetPackPatterns(ObstaclePatternSO[] patterns)
        {
            packPatterns = patterns ?? Array.Empty<ObstaclePatternSO>();
        }
    }
}
