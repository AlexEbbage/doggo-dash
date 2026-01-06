using System.Collections.Generic;
using UnityEngine;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.World.Track;

namespace Game.Presentation.Runtime.World.Pickups
{
    [DisallowMultipleComponent]
    public sealed class SegmentPickupSpawnerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public PickupCatalogSO catalog = default!;
        public PickupPatternSO pattern = default!;

        [Header("Spawn")]
        public Transform? spawnedRoot;

        private readonly List<GameObject> _spawned = new();
        private TrackSegmentBehaviour? _segment;

        private void Awake()
        {
            _segment = GetComponent<TrackSegmentBehaviour>();
            if (_segment == null)
            {
                Debug.LogError("[SegmentPickupSpawner] Must be on same GameObject as TrackSegmentBehaviour.");
                enabled = false;
            }
        }

        private void OnEnable() => SpawnAll();
        private void OnDisable() => Cleanup();

        private void SpawnAll()
        {
            Cleanup();

            if (_segment == null || _segment.laneSockets == null) return;
            if (catalog == null || pattern == null || pattern.rows == null) return;

            Transform root = spawnedRoot != null ? spawnedRoot : transform;

            foreach (var row in pattern.rows)
            {
                if (!catalog.TryGetPrefab(row.type, out var prefab)) continue;

                TrySpawn(prefab, row, root, PickupLaneMask.Left, _segment.laneSockets.left);
                TrySpawn(prefab, row, root, PickupLaneMask.Middle, _segment.laneSockets.middle);
                TrySpawn(prefab, row, root, PickupLaneMask.Right, _segment.laneSockets.right);
            }
        }

        private void TrySpawn(PickupView prefab, PickupPatternSO.Row row, Transform root, PickupLaneMask laneFlag, Transform laneAnchor)
        {
            if ((row.lanes & laneFlag) == 0) return;

            Vector3 pos = laneAnchor.position + new Vector3(0f, 0.5f, row.zOffset);
            var inst = Instantiate(prefab, pos, prefab.transform.rotation, root);

            inst.pickupType = row.type;
            inst.amount = row.amount;

            _spawned.Add(inst.gameObject);
        }

        private void Cleanup()
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null) Destroy(_spawned[i]);
            _spawned.Clear();
        }
    }
}
