using System.Collections.Generic;
using UnityEngine;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.Run;
using Game.Presentation.Runtime.World.Track;

namespace Game.Presentation.Runtime.World.Obstacles
{
    [DisallowMultipleComponent]
    public sealed class SegmentObstacleSpawnerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public ObstacleCatalogSO catalog = default!;
        public ObstaclePatternSO pattern = default!;

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
                Debug.LogError("[SegmentObstacleSpawner] Must be on same GameObject as TrackSegmentBehaviour.");
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

                float p = Mathf.Clamp01(row.chance * density);
                if (p < 1f && Random.value > p) continue;

                if (!catalog.TryGetPrefab(row.obstacleId, out var prefab)) continue;

                TrySpawn(prefab, row.zOffset, row.lanes, root);
            }
        }

        private void TrySpawn(ObstacleView prefab, float zOffset, ObstacleLaneMask lanes, Transform root)
        {
            if ((lanes & ObstacleLaneMask.Left) != 0)
                SpawnInLane(prefab, _segment!.laneSockets!.left, zOffset, root);

            if ((lanes & ObstacleLaneMask.Middle) != 0)
                SpawnInLane(prefab, _segment!.laneSockets!.middle, zOffset, root);

            if ((lanes & ObstacleLaneMask.Right) != 0)
                SpawnInLane(prefab, _segment!.laneSockets!.right, zOffset, root);
        }

        private void SpawnInLane(ObstacleView prefab, Transform laneAnchor, float zOffset, Transform root)
        {
            Vector3 pos = laneAnchor.position + new Vector3(0f, 0f, zOffset);
            var inst = Instantiate(prefab, pos, prefab.transform.rotation, root);
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
