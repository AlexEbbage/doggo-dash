using UnityEngine;

namespace Game.Presentation.Runtime.World.Obstacles
{
    [CreateAssetMenu(menuName = "Game/Configs/Obstacle Catalog", fileName = "ObstacleCatalog")]
    public sealed class ObstacleCatalogSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            [Tooltip("Unique key used by patterns (e.g., RockFull, BranchLow, FenceHigh).")]
            public string id = "RockFull";

            public ObstacleView prefab = default!;
        }

        public Entry[] entries = default!;

        public bool TryGetPrefab(string id, out ObstacleView prefab)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    if (entries[i] != null &&
                        entries[i].prefab != null &&
                        string.Equals(entries[i].id, id, System.StringComparison.OrdinalIgnoreCase))
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
