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

        private readonly List<SpawnedPickup> _spawned = new();
        private readonly Dictionary<PickupView, Stack<PickupView>> _pool = new();
        private TrackSegmentBehaviour? _segment;
        
        private readonly struct SpawnedPickup
        {
            public SpawnedPickup(PickupView prefab, PickupView instance)
            {
                Prefab = prefab;
                Instance = instance;
            }

            public PickupView Prefab { get; }
            public PickupView Instance { get; }
        }

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
            var inst = Rent(prefab, root);
            inst.transform.SetPositionAndRotation(pos, prefab.transform.rotation);
            inst.ResetStartLocalPosition();

            inst.pickupType = row.type;
            inst.amount = row.amount;
            AttachPoolHandle(prefab, inst);

            _spawned.Add(new SpawnedPickup(prefab, inst));
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

        public void ReleaseSpawned(PickupView prefabKey, PickupView instance)
        {
            if (instance == null) return;

            for (int i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i].Instance == instance)
                {
                    _spawned.RemoveAt(i);
                    break;
                }
            }

            Return(prefabKey, instance);
        }

        private PickupView Rent(PickupView prefab, Transform parent)
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

        private void AttachPoolHandle(PickupView prefab, PickupView instance)
        {
            var handle = instance.GetComponent<PickupPoolHandle>();
            if (handle == null)
                handle = instance.gameObject.AddComponent<PickupPoolHandle>();

            handle.Initialize(this, prefab);
        }

        private void Return(PickupView prefabKey, PickupView instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(null, worldPositionStays: true);

            if (!_pool.TryGetValue(prefabKey, out var stack))
            {
                stack = new Stack<PickupView>();
                _pool[prefabKey] = stack;
            }

            stack.Push(instance);
        }
    }
}
