using System.Collections.Generic;
using UnityEngine;

namespace Game.Presentation.Runtime.World.Decor
{
    public sealed class SegmentDecorationSpawnerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public FarmDecorationConfigSO config = default!;

        [Header("Spawn parent")]
        public Transform? spawnedRoot;

        [Header("Determinism")]
        public bool deterministicPerInstance = true;

        private readonly List<GameObject> _spawned = new();
        private readonly List<Vector3> _spawnedPositions = new();

        private int _seed;

        private void Awake()
        {
            if (deterministicPerInstance)
                _seed = gameObject.GetInstanceID();
        }

        private void OnEnable() => Spawn();
        private void OnDisable() => ClearSpawned();

        private void Spawn()
        {
            ClearSpawned();
            if (config == null) return;

            Transform root = spawnedRoot != null ? spawnedRoot : transform;

            var sockets = GetComponentsInChildren<DecorSocketBehaviour>(includeInactive: true);
            if (sockets == null || sockets.Length == 0) return;

            System.Random rng = deterministicPerInstance ? new System.Random(_seed) : new System.Random();

            for (int i = 0; i < sockets.Length; i++)
            {
                var s = sockets[i];
                if (s == null) continue;

                float chance = Mathf.Clamp01(s.spawnChance * config.globalSpawnMultiplier);
                if (rng.NextDouble() > chance) continue;

                var list = GetListFor(s.socketType);
                if (list == null || list.Length == 0) continue;

                var prefab = PickWeighted(list, rng);
                if (prefab == null) continue;

                Vector3 pos = s.transform.position;

                if (config.positionJitterXZ > 0f)
                {
                    float j = config.positionJitterXZ;
                    pos.x += Mathf.Lerp(-j, j, (float)rng.NextDouble());
                    pos.z += Mathf.Lerp(-j, j, (float)rng.NextDouble());
                }

                if (config.minSeparation > 0f && TooClose(pos, config.minSeparation))
                    continue;

                Quaternion rot = s.transform.rotation;
                if (s.randomYaw)
                {
                    float yaw = Mathf.Lerp(0f, 360f, (float)rng.NextDouble());
                    rot = Quaternion.Euler(0f, yaw, 0f) * rot;
                }

                var inst = Instantiate(prefab, pos, rot, root);

                float min = Mathf.Min(s.randomScaleRange.x, s.randomScaleRange.y);
                float max = Mathf.Max(s.randomScaleRange.x, s.randomScaleRange.y);
                float scale = Mathf.Lerp(min, max, (float)rng.NextDouble());
                inst.transform.localScale = inst.transform.localScale * scale;

                _spawned.Add(inst);
                _spawnedPositions.Add(pos);
            }
        }

        private bool TooClose(Vector3 candidate, float minDist)
        {
            float sq = minDist * minDist;
            for (int i = 0; i < _spawnedPositions.Count; i++)
                if ((candidate - _spawnedPositions[i]).sqrMagnitude < sq) return true;
            return false;
        }

        private GameObject PickWeighted(FarmDecorationConfigSO.WeightedPrefab[] arr, System.Random rng)
        {
            float total = 0f;
            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != null && arr[i].prefab != null) total += Mathf.Max(0f, arr[i].weight);

            if (total <= 0.0001f) return null!;

            float roll = (float)rng.NextDouble() * total;
            float acc = 0f;

            for (int i = 0; i < arr.Length; i++)
            {
                var e = arr[i];
                if (e == null || e.prefab == null) continue;

                acc += Mathf.Max(0f, e.weight);
                if (roll <= acc) return e.prefab;
            }

            for (int i = 0; i < arr.Length; i++)
                if (arr[i] != null && arr[i].prefab != null) return arr[i].prefab;

            return null!;
        }

        private FarmDecorationConfigSO.WeightedPrefab[] GetListFor(DecorSocketType type)
        {
            return type switch
            {
                DecorSocketType.GroundEdgeLeft => config.edgeLeftPrefabs,
                DecorSocketType.GroundEdgeRight => config.edgeRightPrefabs,
                DecorSocketType.GroundCenter => config.centerPrefabs,
                DecorSocketType.RaisedLeft => config.raisedLeftPrefabs,
                DecorSocketType.RaisedRight => config.raisedRightPrefabs,
                DecorSocketType.BackgroundNear => config.backgroundNearPrefabs,
                DecorSocketType.BackgroundFar => config.backgroundFarPrefabs,
                _ => config.backgroundNearPrefabs
            };
        }

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawned.Count; i++)
                if (_spawned[i] != null) Destroy(_spawned[i]);
            _spawned.Clear();
            _spawnedPositions.Clear();
        }
    }
}
