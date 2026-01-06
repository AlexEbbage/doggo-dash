using UnityEngine;

namespace Game.Presentation.Runtime.FX
{
    [CreateAssetMenu(menuName = "Game/Configs/VFX Catalog", fileName = "VfxCatalog")]
    public sealed class VfxCatalogSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public VfxId id;
            public GameObject prefab = default!;
            [Min(0f)] public float lifetimeSeconds = 2f;
        }

        public Entry[] entries = default!;

        public bool TryGet(VfxId id, out GameObject prefab, out float lifetimeSeconds)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e != null && e.id == id && e.prefab != null)
                    {
                        prefab = e.prefab;
                        lifetimeSeconds = e.lifetimeSeconds;
                        return true;
                    }
                }
            }

            prefab = null!;
            lifetimeSeconds = 2f;
            return false;
        }
    }
}
