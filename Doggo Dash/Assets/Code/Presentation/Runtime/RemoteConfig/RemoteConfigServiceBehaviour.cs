using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.RemoteConfig
{
    public sealed class RemoteConfigServiceBehaviour : MonoBehaviour, IRemoteConfig
    {
        public RemoteConfigDefaultSO defaults = default!;
        public string streamingAssetsFileName = "remote_config.json";
        public bool allowPlayerPrefsOverrides = true;
        public string playerPrefsPrefix = "rc.";

        public bool logLoadedKeys = false;
        public bool logParseFailures = true;

        private readonly Dictionary<string, string> _values = new Dictionary<string, string>(256);

        private void Awake()
        {
            LoadDefaults();
            StartCoroutine(LoadStreamingAssetsJson());
        }

        private void LoadDefaults()
        {
            _values.Clear();
            if (defaults == null || defaults.entries == null) return;

            for (int i = 0; i < defaults.entries.Length; i++)
            {
                var e = defaults.entries[i];
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.key)) continue;
                _values[e.key] = e.value ?? string.Empty;
            }
        }

        private IEnumerator LoadStreamingAssetsJson()
        {
            if (string.IsNullOrWhiteSpace(streamingAssetsFileName))
                yield break;

            string path = System.IO.Path.Combine(Application.streamingAssetsPath, streamingAssetsFileName);

#if UNITY_ANDROID && !UNITY_EDITOR
            using var req = UnityWebRequest.Get(path);
#else
            using var req = UnityWebRequest.Get("file://" + path);
#endif
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
                yield break;

            string json = req.downloadHandler.text;
            if (string.IsNullOrWhiteSpace(json))
                yield break;

            if (!TryParseSimpleJsonDictionary(json, out var dict))
            {
                if (logParseFailures)
                    Debug.LogWarning("[RemoteConfig] Failed to parse streaming JSON. Expected flat object like { \"k\": \"v\", \"n\": 123, \"b\": true }");
                yield break;
            }

            foreach (var kv in dict)
                if (!string.IsNullOrWhiteSpace(kv.Key))
                    _values[kv.Key] = kv.Value ?? string.Empty;

            if (logLoadedKeys)
                Debug.Log($"[RemoteConfig] Loaded {dict.Count} keys from StreamingAssets.");
        }

        public void SetOverride(string key, string value)
        {
            if (!allowPlayerPrefsOverrides) return;
            if (string.IsNullOrWhiteSpace(key)) return;

            string prefsKey = playerPrefsPrefix + key;
            PlayerPrefs.SetString(prefsKey, value ?? string.Empty);
            PlayerPrefs.Save();
            _values[key] = value ?? string.Empty;
        }

        public bool HasKey(string key) => !string.IsNullOrWhiteSpace(key) && _values.ContainsKey(key);

        public int GetInt(string key, int defaultValue)
        {
            if (!TryGetRaw(key, out var raw)) return defaultValue;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        public float GetFloat(string key, float defaultValue)
        {
            if (!TryGetRaw(key, out var raw)) return defaultValue;
            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : defaultValue;
        }

        public bool GetBool(string key, bool defaultValue)
        {
            if (!TryGetRaw(key, out var raw)) return defaultValue;
            if (bool.TryParse(raw, out var b)) return b;
            if (raw == "1") return true;
            if (raw == "0") return false;
            return defaultValue;
        }

        public string GetString(string key, string defaultValue)
        {
            if (!TryGetRaw(key, out var raw)) return defaultValue;
            return raw ?? defaultValue;
        }

        private bool TryGetRaw(string key, out string raw)
        {
            raw = null;
            if (string.IsNullOrWhiteSpace(key)) return false;

            if (allowPlayerPrefsOverrides)
            {
                string prefsKey = playerPrefsPrefix + key;
                if (PlayerPrefs.HasKey(prefsKey))
                {
                    raw = PlayerPrefs.GetString(prefsKey, string.Empty);
                    _values[key] = raw;
                    return true;
                }
            }

            return _values.TryGetValue(key, out raw);
        }

        private static bool TryParseSimpleJsonDictionary(string json, out Dictionary<string, string> dict)
        {
            dict = new Dictionary<string, string>(64);

            int i = 0;
            SkipWs(json, ref i);
            if (i >= json.Length || json[i] != '{') return false;
            i++;

            while (i < json.Length)
            {
                SkipWs(json, ref i);
                if (i < json.Length && json[i] == '}') return true;

                if (!ReadQuoted(json, ref i, out var key)) return false;

                SkipWs(json, ref i);
                if (i >= json.Length || json[i] != ':') return false;
                i++;

                SkipWs(json, ref i);

                string val;
                if (i < json.Length && json[i] == '"')
                {
                    if (!ReadQuoted(json, ref i, out val)) return false;
                }
                else
                {
                    if (!ReadToken(json, ref i, out val)) return false;
                }

                dict[key] = val ?? string.Empty;

                SkipWs(json, ref i);
                if (i < json.Length && json[i] == ',') { i++; continue; }
                SkipWs(json, ref i);
                if (i < json.Length && json[i] == '}') return true;
            }

            return false;
        }

        private static void SkipWs(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static bool ReadQuoted(string s, ref int i, out string result)
        {
            result = null;
            SkipWs(s, ref i);
            if (i >= s.Length || s[i] != '"') return false;
            i++;
            int start = i;
            while (i < s.Length)
            {
                if (s[i] == '\\') { i += 2; continue; }
                if (s[i] == '"') break;
                i++;
            }
            if (i >= s.Length) return false;
            result = s.Substring(start, i - start);
            i++;
            return true;
        }

        private static bool ReadToken(string s, ref int i, out string result)
        {
            result = null;
            SkipWs(s, ref i);
            if (i >= s.Length) return false;
            int start = i;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == ',' || c == '}' || char.IsWhiteSpace(c)) break;
                i++;
            }
            if (i <= start) return false;
            result = s.Substring(start, i - start);
            return true;
        }
    }
}
