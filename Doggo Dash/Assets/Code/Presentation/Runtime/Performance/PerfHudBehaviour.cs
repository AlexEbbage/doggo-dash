using UnityEngine;
using TMPro;

namespace Game.Presentation.Runtime.Performance
{
    public sealed class PerfHudBehaviour : MonoBehaviour
    {
        public RunPerformanceMonitorBehaviour monitor = default!;
        public TMP_Text label;
        public bool visible = true;
        [Min(0.1f)] public float updateIntervalSeconds = 0.5f;

        private float _t;

        private void Awake()
        {
            if (label != null) label.gameObject.SetActive(visible);
        }

        private void Update()
        {
            if (!visible || label == null || monitor == null) return;

            _t += Time.unscaledDeltaTime;
            if (_t < updateIntervalSeconds) return;
            _t = 0f;

            label.text =
                $"Tier: {monitor.DeviceTierName}\n" +
                $"FPS~ {Mathf.RoundToInt(monitor.RollingFps)} (min~ {Mathf.RoundToInt(monitor.RollingMinFps)})\n" +
                $"Hitch max: {Mathf.RoundToInt(monitor.MaxFrameTimeMs)}ms\n" +
                $"Mem max: {FormatBytes(monitor.ManagedMemMax)}";
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes <= 0) return "0";
            float mb = bytes / (1024f * 1024f);
            return $"{mb:F1}MB";
        }
    }
}
