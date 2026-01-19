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

        private readonly List<SpawnedObstacle> _spawned = new();
        private readonly Dictionary<ObstacleView, Stack<ObstacleView>> _pool = new();
        private TrackSegmentBehaviour? _segment;

        private readonly struct SpawnedObstacle
        {
            public SpawnedObstacle(ObstacleView prefab, ObstacleView instance)
            {
                Prefab = prefab;
                Instance = instance;
            }

            public ObstacleView Prefab { get; }
            public ObstacleView Instance { get; }
        }

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
            if (catalog == null || pattern == null) return;

            Transform root = spawnedRoot != null ? spawnedRoot : transform;
            float density = difficulty != null ? difficulty.DensityMultiplier : 1f;

            var rows = pattern.GetRows();
            if (rows == null) return;

            foreach (var row in rows)
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
            var inst = Rent(prefab, root);
            inst.transform.SetPositionAndRotation(pos, prefab.transform.rotation);
            _spawned.Add(new SpawnedObstacle(prefab, inst));
        }

        private void Cleanup()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i].Instance != null)
                    Return(_spawned[i].Prefab, _spawned[i].Instance);
            }
            _spawned.Clear();
        }

        private ObstacleView Rent(ObstacleView prefab, Transform parent)
        {
            if (_pool.TryGetValue(prefab, out var stack) && stack.Count > 0)
            {
                var inst = stack.Pop();
                inst.gameObject.SetActive(true);
                inst.transform.SetParent(parent, worldPositionStays: true);
                return inst;
            }

            return Instantiate(prefab, parent);
        }

        private void Return(ObstacleView prefabKey, ObstacleView instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(null, worldPositionStays: true);

            if (!_pool.TryGetValue(prefabKey, out var stack))
            {
                stack = new Stack<ObstacleView>();
                _pool[prefabKey] = stack;
            }

            stack.Push(instance);
        }
    }
}
