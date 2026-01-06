using UnityEngine;
using Game.Presentation.Runtime.Analytics;
using Game.Presentation.Runtime.RemoteConfig;
using Game.Presentation.Runtime.Events;
using Game.Presentation.Runtime.CloudSave;

namespace Game.Presentation.Runtime.Lifecycle
{
    public sealed class SceneServicesGuardBehaviour : MonoBehaviour
    {
        public bool autoDisableDuplicates = false;

        [Header("Checks")]
        public bool checkAnalytics = true;
        public bool checkRemoteConfig = true;
        public bool checkEvents = true;
        public bool checkCloudSave = true;

        private void Awake()
        {
            if (checkAnalytics) Check<AnalyticsServiceBehaviour>();
            if (checkRemoteConfig) Check<RemoteConfigServiceBehaviour>();
            if (checkEvents) Check<EventRuntimeServiceBehaviour>();
            if (checkCloudSave) Check<CloudSaveServiceBehaviour>();
        }

        private void Check<T>() where T : Behaviour
        {
            var arr = FindObjectsOfType<T>(true);
            if (arr == null || arr.Length <= 1) return;

            Debug.LogWarning($"[ServicesGuard] Found {arr.Length} instances of {typeof(T).Name} in scene. Consider removing duplicates.");

            if (!autoDisableDuplicates) return;

            bool kept = false;
            for (int i = 0; i < arr.Length; i++)
            {
                var b = arr[i];
                if (b == null) continue;
                if (!kept && b.enabled)
                {
                    kept = true;
                    continue;
                }
                b.enabled = false;
            }
        }
    }
}
