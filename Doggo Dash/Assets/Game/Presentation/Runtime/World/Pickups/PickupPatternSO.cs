using UnityEngine;
using Game.Domain.ValueObjects;
using System;

namespace Game.Presentation.Runtime.World.Pickups
{
    [CreateAssetMenu(menuName = "Game/Configs/Pickup Pattern", fileName = "PickupPattern")]
    public sealed class PickupPatternSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Row
        {
            public float zOffset = 5f;
            public PickupLaneMask lanes = PickupLaneMask.Middle;
            public PickupType type = PickupType.Treat;
            [Min(1)] public int amount = 1;
        }

        public Row[] rows = default!;

        [SerializeField, HideInInspector]
        private PickupPatternSO[] packPatterns = Array.Empty<PickupPatternSO>();

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

        public void SetPackPatterns(PickupPatternSO[] patterns)
        {
            packPatterns = patterns ?? Array.Empty<PickupPatternSO>();
        }
    }
}
