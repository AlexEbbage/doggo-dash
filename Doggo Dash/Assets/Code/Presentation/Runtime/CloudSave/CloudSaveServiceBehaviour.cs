using System;
using UnityEngine;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using Game.Infrastructure.Serialization;

namespace Game.Presentation.Runtime.CloudSave
{
    public sealed class CloudSaveServiceBehaviour : MonoBehaviour
    {
        public MonoBehaviour providerBehaviour = default!;
        public string slotKey = "progress_v1";
        public bool autoSyncOnStart = false;
        public bool log = true;

        private ICloudSaveProvider _provider = default!;
        private IProgressSaveGateway _localSave;

        public bool IsReady => _provider != null && _provider.IsReady;
        public event Action<string>? OnStatus;

        private void Awake()
        {
            _provider = providerBehaviour as ICloudSaveProvider;
            if (_provider == null)
            {
                Debug.LogError("[CloudSave] providerBehaviour must implement ICloudSaveProvider.");
                enabled = false;
                return;
            }

            _localSave = new PlayerPrefsProgressSaveGateway();

            _provider.Initialize(
                onReady: () =>
                {
                    Notify($"Cloud ready: {_provider.ProviderName}");
                    if (autoSyncOnStart) SyncDownThenMerge();
                },
                onError: err => Notify($"Cloud init error: {err}")
            );
        }

        public void SyncUp()
        {
            if (!IsReady) { Notify("Cloud not ready"); return; }

            var local = _localSave.Load();
            EnsureMeta(local);

            Notify("Uploading...");
            _provider.Upload(slotKey, ProgressJsonSerializer.ToJson(local),
                onSuccess: () => Notify("Upload complete"),
                onError: err => Notify($"Upload failed: {err}")
            );
        }

        public void SyncDownThenMerge()
        {
            if (!IsReady) { Notify("Cloud not ready"); return; }

            Notify("Downloading...");
            _provider.Download(slotKey,
                onSuccessJson: json =>
                {
                    if (!ProgressJsonSerializer.TryFromJson(json, out var cloud))
                    {
                        Notify("Cloud data invalid/empty");
                        return;
                    }

                    var local = _localSave.Load();
                    EnsureMeta(local);
                    EnsureMeta(cloud);

                    var merged = ProgressMergeUtility.Merge(local, cloud);
                    _localSave.Save(merged);
                    Notify("Merge complete");
                },
                onError: err => Notify($"Download failed: {err}")
            );
        }

        private void EnsureMeta(PlayerProgressData data)
        {
            if (data == null) return;
            if (string.IsNullOrWhiteSpace(data.deviceId))
                data.deviceId = SystemInfo.deviceUniqueIdentifier;
            if (data.lastModifiedUnixUtc <= 0)
                data.lastModifiedUnixUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void Notify(string msg)
        {
            OnStatus?.Invoke(msg);
            if (log) Debug.Log($"[CloudSave] {msg}");
        }
    }
}
