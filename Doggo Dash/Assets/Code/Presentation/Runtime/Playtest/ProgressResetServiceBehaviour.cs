using UnityEngine;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Playtest
{
    /// <summary>
    /// Targeted progress reset for playtesting.
    /// Avoids PlayerPrefs.DeleteAll() so settings aren't wiped.
    /// </summary>
    public sealed class ProgressResetServiceBehaviour : MonoBehaviour
    {
        [Header("Progress Keys")]
        [Tooltip("PlayerPrefs keys to delete for a full progress reset. Add your gateway's main JSON key here.")]
        public string[] progressKeys =
        {
            "player_progress" // <-- Change to your actual gateway key if different
        };

        [Header("Optional: also clear these")]
        [Tooltip("If true, clears RemoteConfig QA overrides stored under the given prefix.")]
        public bool clearRemoteConfigOverrides = true;

        [Tooltip("RemoteConfig override prefix (matches RemoteConfigServiceBehaviour.playerPrefsPrefix)")]
        public string remoteConfigPrefix = "rc.";

        [Tooltip("If true, clears the PlayerPrefs analytics buffer (if used).")]
        public bool clearAnalyticsBuffer = true;

        [Tooltip("Analytics buffer key (matches PlayerPrefsAnalyticsSinkBehaviour.prefsKey)")]
        public string analyticsBufferKey = "analytics_buffer";

        private IProgressSaveGateway _save;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
        }

        public void ResetAllProgress()
        {
            if (progressKeys != null)
            {
                for (int i = 0; i < progressKeys.Length; i++)
                {
                    var k = progressKeys[i];
                    if (!string.IsNullOrWhiteSpace(k))
                        PlayerPrefs.DeleteKey(k);
                }
            }

            // Clear RemoteConfig overrides deterministically if you track keys under rc.__keys (newline-separated).
            if (clearRemoteConfigOverrides)
            {
                string listKey = remoteConfigPrefix + "__keys";
                if (PlayerPrefs.HasKey(listKey))
                {
                    string joined = PlayerPrefs.GetString(listKey, string.Empty);
                    var keys = joined.Split('\n');
                    for (int i = 0; i < keys.Length; i++)
                    {
                        var k = keys[i];
                        if (string.IsNullOrWhiteSpace(k)) continue;
                        PlayerPrefs.DeleteKey(remoteConfigPrefix + k);
                    }
                    PlayerPrefs.DeleteKey(listKey);
                }
            }

            if (clearAnalyticsBuffer && !string.IsNullOrWhiteSpace(analyticsBufferKey))
                PlayerPrefs.DeleteKey(analyticsBufferKey);

            PlayerPrefs.Save();

            Debug.Log("[Playtest] Progress reset (targeted PlayerPrefs keys deleted).");
        }

        public PlayerProgressData Load() => _save.Load();
    }
}
