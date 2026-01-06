using UnityEngine;

namespace Game.Presentation.Runtime.FX
{
    public sealed class VfxSpawnerBehaviour : MonoBehaviour
    {
        public VfxCatalogSO catalog = default!;

        public void Spawn(VfxId id, Vector3 position, Quaternion rotation)
        {
            if (catalog == null) return;
            if (!catalog.TryGet(id, out var prefab, out var lifetime)) return;

            var go = Instantiate(prefab, position, rotation);
            if (lifetime > 0f) Destroy(go, lifetime);
        }

        public void Spawn(VfxId id, Vector3 position)
        {
            Spawn(id, position, Quaternion.identity);
        }
    }
}
