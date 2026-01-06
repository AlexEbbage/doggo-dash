using System.Collections.Generic;
using UnityEngine;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.World.Track;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.World.Pickups
{
    [DisallowMultipleComponent]
    public sealed class SegmentPickupSpawnerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public PickupCatalogSO catalog = default!;
        public PickupPatternSO pattern = default!;

        [Header("Difficulty (optional)")]
        public RunDifficultyControllerBehaviour difficulty;

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
                return;
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
            float density = difficulty != null ? difficulty.DensityMultiplier : 1f;

            foreach (var row in pattern.rows)
            {
                if (row == null) continue;
                if (!catalog.TryGetPrefab(row.type, out var prefab)) continue;

                if (density < 1f && Random.value > density)
                    continue;

                TrySpawnInLaneMask(prefab, row, root, PickupLaneMask.Left, -1);
                TrySpawnInLaneMask(prefab, row, root, PickupLaneMask.Middle, 0);
                TrySpawnInLaneMask(prefab, row, root, PickupLaneMask.Right, +1);
            }
        }

        private void TrySpawnInLaneMask(PickupView prefab, PickupPatternSO.Row row, Transform root, PickupLaneMask laneFlag, int laneIndex)
        {
            if ((row.lanes & laneFlag) == 0) return;

            Transform laneAnchor = laneIndex switch
            {
                -1 => _segment!.laneSockets!.left,
                0 => _segment!.laneSockets!.middle,
                _ => _segment!.laneSockets!.right
            };

            Vector3 pos = laneAnchor.position + new Vector3(0f, 0.5f, row.zOffset);
            var inst = Instantiate(prefab, pos, prefab.transform.rotation, root);

            inst.pickupType = row.type;
            inst.amount = row.amount;

            _spawned.Add(inst.gameObject);
        }

        private void Cleanup()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                    Destroy(_spawned[i]);
            }
            _spawned.Clear();
        }
    }
}
