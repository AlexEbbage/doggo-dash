using UnityEngine;

namespace Game.Presentation.Runtime.World.Pickups
{
    public sealed class PickupPoolHandle : MonoBehaviour
    {
        private SegmentPickupSpawnerBehaviour _spawner;
        private PickupView _prefabKey;

        public void Initialize(SegmentPickupSpawnerBehaviour spawner, PickupView prefabKey)
        {
            _spawner = spawner;
            _prefabKey = prefabKey;
        }

        public void Release(PickupView instance)
        {
            if (_spawner == null || _prefabKey == null) return;
            _spawner.ReleaseSpawned(_prefabKey, instance);
        }
    }
}
