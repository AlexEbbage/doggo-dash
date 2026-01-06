using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.World.Pickups
{
    [CreateAssetMenu(menuName = "Game/Configs/Pickup Catalog", fileName = "PickupCatalog")]
    public sealed class PickupCatalogSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public PickupType type;
            public PickupView prefab = default!;
        }

        public Entry[] entries = default!;

        public bool TryGetPrefab(PickupType type, out PickupView prefab)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i].type == type && entries[i].prefab != null)
                    {
                        prefab = entries[i].prefab;
                        return true;
                    }
                }
            }

            prefab = null!;
            return false;
        }
    }
}
