using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Application.Ports;
using Game.Presentation.Runtime.Analytics;

namespace Game.Presentation.Runtime.CrashReporting
{
    public sealed class CrashReportingServiceBehaviour : MonoBehaviour
    {
        [Header("Provider (must implement ICrashReporter)")]
        public MonoBehaviour providerBehaviour = default!;

        [Header("Optional Analytics")]
        public AnalyticsServiceBehaviour analytics;

        [Header("Options")]
        public bool captureUnityLogExceptions = true;
        public bool captureUnhandledExceptions = true;
        public bool captureUnobservedTaskExceptions = true;

        [Min(1)] public int breadcrumbCapacity = 40;
        [Min(16)] public int breadcrumbMaxChars = 80;

        [Header("Dedupe")]
        [Min(16)] public int dedupeCapacity = 64;
        [Min(0.5f)] public float dedupeWindowSeconds = 2.0f;

        private ICrashReporter _provider;
        private readonly Queue<string> _breadcrumbs = new();
        private CrashDedupeCache _dedupe;

        private void Awake()
        {
            _provider = providerBehaviour as ICrashReporter;
            if (_provider == null)
            {
                Debug.LogError("[CrashReporting] providerBehaviour must implement ICrashReporter.");
                enabled = false;
                return;
            }

            _dedupe = new CrashDedupeCache(dedupeCapacity, dedupeWindowSeconds);

            _provider.SetUserProperty("platform", Application.platform.ToString());
            _provider.SetUserProperty("version", Application.version);
            _provider.SetUserProperty("device_model", SystemInfo.deviceModel);

            if (captureUnityLogExceptions)
                Application.logMessageReceivedThreaded += OnUnityLogThreaded;

            if (captureUnhandledExceptions)
                AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            if (captureUnobservedTaskExceptions)
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            Breadcrumb($"CrashReporting init scene={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }

        private void OnDestroy()
        {
            if (captureUnityLogExceptions)
                Application.logMessageReceivedThreaded -= OnUnityLogThreaded;

            if (captureUnhandledExceptions)
                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

            if (captureUnobservedTaskExceptions)
                System.Threading.Tasks.TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
        }

        public void Breadcrumb(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            message = Trunc(message, Mathf.Max(16, breadcrumbMaxChars));

            EnqueueBreadcrumb(message);
            _provider.AddBreadcrumb(message);
        }

        public void ReportError(string message, IReadOnlyDictionary<string, object> context = null)
        {
            if (string.IsNullOrWhiteSpace(message)) message = "(null)";
            if (!ShouldReport(message, null)) return;

            var ctx = AttachBreadcrumbs(context);
            _provider.RecordError(message, ctx);

            if (analytics != null)
            {
                analytics.Track("error_reported", new Dictionary<string, object>
                {
                    { "message", Trunc(message, 160) },
                    { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
                });
            }
        }

        public void ReportException(Exception ex, IReadOnlyDictionary<string, object> context = null)
        {
            string type = ex != null ? ex.GetType().Name : "null";
            string msg = ex != null ? ex.Message : "null";
            if (!ShouldReport(type, msg)) return;

            var ctx = AttachBreadcrumbs(context);
            _provider.RecordException(ex, ctx);

            if (analytics != null)
            {
                analytics.Track("exception_reported", new Dictionary<string, object>
                {
                    { "type", type },
                    { "message", Trunc(msg, 160) },
                    { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
                });
            }
        }

        private void OnUnityLogThreaded(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Exception && type != LogType.Assert && type != LogType.Error)
                return;

            if (!ShouldReport(condition, stackTrace)) return;

            var ctx = new Dictionary<string, object>
            {
                { "unity_log_type", type.ToString() },
                { "condition", Trunc(condition, 220) },
                { "stack", Trunc(stackTrace, 400) },
                { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
            };

            _provider.RecordError($"UnityLog {type}: {Trunc(condition, 220)}", AttachBreadcrumbs(ctx));

            if (analytics != null)
            {
                analytics.Track("unity_log_error", new Dictionary<string, object>
                {
                    { "type", type.ToString() },
                    { "condition", Trunc(condition, 160) }
                });
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                var ctx = new Dictionary<string, object>
                {
                    { "is_terminating", e.IsTerminating },
                    { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
                };

                ReportException(ex ?? new Exception("Unhandled exception (non-Exception object)"), ctx);
            }
            catch { }
        }

        private void OnUnobservedTaskException(object sender, System.Threading.Tasks.UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                var ctx = new Dictionary<string, object>
                {
                    { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
                };

                ReportException(e.Exception, ctx);
                e.SetObserved();
            }
            catch { }
        }

        private bool ShouldReport(string a, string b)
        {
            float now = Time.realtimeSinceStartup;
            int h = 17;
            h = (h * 31) + (a != null ? a.GetHashCode() : 0);
            h = (h * 31) + (b != null ? b.GetHashCode() : 0);
            return _dedupe.ShouldReport(h, now);
        }

        private void EnqueueBreadcrumb(string message)
        {
            while (_breadcrumbs.Count >= Mathf.Max(1, breadcrumbCapacity))
                _breadcrumbs.Dequeue();
            _breadcrumbs.Enqueue(message);
        }

        private IReadOnlyDictionary<string, object> AttachBreadcrumbs(IReadOnlyDictionary<string, object> context)
        {
            var dict = new Dictionary<string, object>(16);

            if (context != null)
            {
                foreach (var kv in context)
                    dict[kv.Key] = kv.Value;
            }

            dict["breadcrumbs"] = string.Join(" > ", _breadcrumbs);
            return dict;
        }

        private static string Trunc(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return s ?? "";
            if (s.Length <= max) return s;
            return s.Substring(0, max);
        }
    }
}
