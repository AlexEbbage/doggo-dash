using UnityEngine;
using Game.Domain.ValueObjects;

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
    }
}
