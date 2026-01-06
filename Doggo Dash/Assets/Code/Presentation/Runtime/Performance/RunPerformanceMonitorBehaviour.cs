using System;
using System.Collections.Generic;
using System.Runtime;
using UnityEngine;
using Game.Presentation.Runtime.Analytics;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Performance
{
    public sealed class RunPerformanceMonitorBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public AnalyticsServiceBehaviour analytics = default!;
        public RunStateControllerBehaviour runState = default!;
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        [Header("Sampling")]
        [Min(0.1f)] public float sampleIntervalSeconds = 1.0f;
        public bool sendPeriodicSamples = false;

        [Header("GC")]
        public bool trackGcCollections = true;

        [Header("Debug")]
        public bool logSummaryToConsole = true;

        public string DeviceTierName => _tier.ToString();
        public float RollingFps => _avgFps;
        public float RollingMinFps => float.IsInfinity(_minFps) ? 0f : _minFps;
        public float MaxFrameTimeMs => _maxFrameTimeMs;
        public long ManagedMemMax => _managedMemMax;

        private DeviceTier _tier;
        private bool _runEndedSent;

        private int _frames;
        private float _timeAccum;
        private float _minFps = float.PositiveInfinity;
        private float _avgFps;

        private float _nextSampleAt;

        private int _gc0Start, _gc1Start, _gc2Start;

        private long _managedMemMin = long.MaxValue;
        private long _managedMemMax = 0;

        private long _monoHeapSize;
        private long _monoUsedSize;

        private float _maxFrameTimeMs;

        private readonly Dictionary<string, object> _dict = new(32);

        private void Awake()
        {
            _tier = DeviceTierClassifier.Classify();
            ResetRun();

            if (trackGcCollections)
            {
                _gc0Start = GC.CollectionCount(0);
                _gc1Start = GC.CollectionCount(1);
                _gc2Start = GC.CollectionCount(2);
            }

            if (analytics != null)
            {
                _dict.Clear();
                _dict["device_tier"] = _tier.ToString();
                _dict["ram_mb"] = SystemInfo.systemMemorySize;
                _dict["cores"] = SystemInfo.processorCount;
                _dict["gpu"] = SystemInfo.graphicsDeviceName;
                _dict["gpu_vram_mb"] = SystemInfo.graphicsMemorySize;
                _dict["quality"] = QualitySettings.names != null && QualitySettings.names.Length > QualitySettings.GetQualityLevel()
                    ? QualitySettings.names[QualitySettings.GetQualityLevel()]
                    : QualitySettings.GetQualityLevel().ToString();
                analytics.Track("perf_device_info", _dict);
            }
        }

        public void ResetRun()
        {
            _runEndedSent = false;

            _frames = 0;
            _timeAccum = 0f;
            _minFps = float.PositiveInfinity;
            _avgFps = 0f;

            _nextSampleAt = Time.unscaledTime + sampleIntervalSeconds;

            _managedMemMin = long.MaxValue;
            _managedMemMax = 0;

            _maxFrameTimeMs = 0f;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            float dtMs = dt * 1000f;
            if (dtMs > _maxFrameTimeMs) _maxFrameTimeMs = dtMs;

            _frames++;
            _timeAccum += dt;

            if (_timeAccum >= 0.25f)
            {
                float fps = _frames / Mathf.Max(0.0001f, _timeAccum);
                _avgFps = fps;
                if (fps < _minFps) _minFps = fps;

                _frames = 0;
                _timeAccum = 0f;

                SampleMemory();
            }

            if (sendPeriodicSamples && Time.unscaledTime >= _nextSampleAt)
            {
                _nextSampleAt = Time.unscaledTime + sampleIntervalSeconds;
                SendPeriodicSample();
            }

            if (!_runEndedSent && runState != null && runState.IsFailed)
            {
                _runEndedSent = true;
                SendRunSummary();
            }
        }

        private void SampleMemory()
        {
            long managed = GC.GetTotalMemory(false);
            if (managed < _managedMemMin) _managedMemMin = managed;
            if (managed > _managedMemMax) _managedMemMax = managed;

#if UNITY_2020_2_OR_NEWER
            _monoHeapSize = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
            _monoUsedSize = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
#endif
        }

        private void SendPeriodicSample()
        {
            if (analytics == null) return;

            _dict.Clear();
            _dict["device_tier"] = _tier.ToString();
            _dict["fps_roll"] = Mathf.RoundToInt(_avgFps);
            _dict["fps_min_roll"] = float.IsInfinity(_minFps) ? 0 : Mathf.RoundToInt(_minFps);
            _dict["max_dt_ms"] = Mathf.RoundToInt(_maxFrameTimeMs);
            _dict["managed_mem_max"] = _managedMemMax;

#if UNITY_2020_2_OR_NEWER
            _dict["mono_heap"] = _monoHeapSize;
            _dict["mono_used"] = _monoUsedSize;
#endif

            analytics.Track("perf_sample", _dict);
        }

        private void SendRunSummary()
        {
            int dist = scoreDistance != null ? Mathf.FloorToInt(scoreDistance.DistanceMeters) : 0;
            int score = scoreDistance != null ? scoreDistance.Score : 0;

            int gc0 = 0, gc1 = 0, gc2 = 0;
            if (trackGcCollections)
            {
                gc0 = GC.CollectionCount(0) - _gc0Start;
                gc1 = GC.CollectionCount(1) - _gc1Start;
                gc2 = GC.CollectionCount(2) - _gc2Start;
            }

            long memMin = _managedMemMin == long.MaxValue ? GC.GetTotalMemory(false) : _managedMemMin;
            long memMax = Mathf.Max((long)0, _managedMemMax);

            if (analytics != null)
            {
                _dict.Clear();
                _dict["device_tier"] = _tier.ToString();
                _dict["distance_m"] = dist;
                _dict["score"] = score;

                _dict["fps_min_roll"] = float.IsInfinity(_minFps) ? 0 : Mathf.RoundToInt(_minFps);
                _dict["fps_last_roll"] = Mathf.RoundToInt(_avgFps);

                _dict["max_dt_ms"] = Mathf.RoundToInt(_maxFrameTimeMs);

                _dict["managed_mem_min"] = memMin;
                _dict["managed_mem_max"] = memMax;

#if UNITY_2020_2_OR_NEWER
                _dict["mono_heap"] = _monoHeapSize;
                _dict["mono_used"] = _monoUsedSize;
#endif

                _dict["gc0"] = gc0;
                _dict["gc1"] = gc1;
                _dict["gc2"] = gc2;

                analytics.Track("perf_run_summary", _dict);
            }

            if (logSummaryToConsole)
            {
                Debug.Log(
                    $"[PERF] tier={_tier} dist={dist} score={score} fps(last)={Mathf.RoundToInt(_avgFps)} fps(minRoll)={(float.IsInfinity(_minFps) ? 0 : Mathf.RoundToInt(_minFps))} " +
                    $"maxDtMs={Mathf.RoundToInt(_maxFrameTimeMs)} mem(min)={memMin} mem(max)={memMax} gc0={gc0} gc1={gc1} gc2={gc2}");
            }
        }
    }
}
