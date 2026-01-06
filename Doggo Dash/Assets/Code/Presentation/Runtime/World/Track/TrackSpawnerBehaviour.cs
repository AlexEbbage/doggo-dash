using System.Collections.Generic;
using UnityEngine;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.World.Track
{
    /// <summary>
    /// Hardened spawner: uses runner travelled distance (meters) rather than world Z.
    /// Straight-run compatible via RunnerDistanceProviderBehaviour.
    /// </summary>
    public sealed class TrackSpawnerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public TrackBiomeConfigSO biomeConfig = default!;

        [Header("Refs")]
        public MonoBehaviour runnerDistanceProviderBehaviour = default!;
        public Transform segmentsRoot = default!;

        private IRunnerDistanceProvider _runnerDistance = default!;

        private readonly Queue<AliveSeg> _alive = new();
        private readonly TrackSegmentPool _pool = new();

        private System.Random _rng;
        private Transform? _lastEndSocket;
        private float _lastEndDistance;

        private struct AliveSeg
        {
            public TrackSegmentBehaviour PrefabKey;
            public TrackSegmentBehaviour Instance;
            public Transform EndSocket;
            public float EndDistance;
        }

        private void Awake()
        {
            _rng = new System.Random();

            _runnerDistance = runnerDistanceProviderBehaviour as IRunnerDistanceProvider;
            if (_runnerDistance == null)
            {
                Debug.LogError("[TrackSpawner] runnerDistanceProviderBehaviour must implement IRunnerDistanceProvider.");
                enabled = false;
            }
        }

        private void Start()
        {
            if (biomeConfig == null || biomeConfig.segmentPrefabs == null || biomeConfig.segmentPrefabs.Length == 0)
            {
                Debug.LogError("[TrackSpawner] No segment prefabs configured.");
                enabled = false;
                return;
            }

            if (segmentsRoot == null) segmentsRoot = transform;

            _lastEndDistance = 0f;
            _lastEndSocket = null;

            for (int i = 0; i < biomeConfig.initialSegments; i++)
                SpawnNext();
        }

        private void Update()
        {
            float runnerDist = _runnerDistance.DistanceTravelledMeters;

            float distToEnd = _lastEndDistance - runnerDist;
            if (distToEnd < biomeConfig.spawnAheadThreshold && _alive.Count < biomeConfig.maxAliveSegments)
                SpawnNext();

            while (_alive.Count > 0)
            {
                AliveSeg seg = _alive.Peek();
                float behind = runnerDist - seg.EndDistance;
                if (behind < biomeConfig.despawnBehindDistance) break;

                _alive.Dequeue();
                _pool.Return(seg.PrefabKey, seg.Instance);
            }
        }

        private void SpawnNext()
        {
            TrackSegmentBehaviour prefab = PickPrefab();
            TrackSegmentBehaviour seg = _pool.Rent(prefab, segmentsRoot);

            AlignSegment(seg);

            Transform endSocket = seg.EndSocket;

            float segLen = Vector3.Distance(seg.StartSocket.position, endSocket.position);
            if (segLen < 0.01f) segLen = 0.01f;

            float startDist = _lastEndDistance;
            float endDist = startDist + segLen;

            _alive.Enqueue(new AliveSeg
            {
                PrefabKey = prefab,
                Instance = seg,
                EndSocket = endSocket,
                EndDistance = endDist
            });

            _lastEndSocket = endSocket;
            _lastEndDistance = endDist;
        }

        private TrackSegmentBehaviour PickPrefab()
        {
            int idx = _rng.Next(0, biomeConfig.segmentPrefabs.Length);
            return biomeConfig.segmentPrefabs[idx];
        }

        private void AlignSegment(TrackSegmentBehaviour seg)
        {
            if (_lastEndSocket == null)
            {
                Vector3 startPos = seg.StartSocket.position;
                seg.transform.position += (Vector3.zero - startPos);
                return;
            }

            Transform start = seg.StartSocket;
            Transform target = _lastEndSocket;

            Quaternion rotDelta = target.rotation * Quaternion.Inverse(start.rotation);
            seg.transform.rotation = rotDelta * seg.transform.rotation;

            Vector3 posDelta = target.position - start.position;
            seg.transform.position += posDelta;
        }
    }
}
