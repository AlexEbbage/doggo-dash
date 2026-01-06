using System;
using System.IO;
using UnityEngine;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.CloudSave
{
    public sealed class LocalFileCloudSaveProviderBehaviour : MonoBehaviour, ICloudSaveProvider
    {
        public bool IsReady { get; private set; }
        public string ProviderName => "LocalFile";

        public void Initialize(Action onReady, Action<string> onError)
        {
            IsReady = true;
            onReady?.Invoke();
        }

        public void Upload(string slotKey, string json, Action onSuccess, Action<string> onError)
        {
            try
            {
                File.WriteAllText(GetPath(slotKey), json ?? string.Empty);
                onSuccess?.Invoke();
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }

        public void Download(string slotKey, Action<string> onSuccessJson, Action<string> onError)
        {
            try
            {
                var path = GetPath(slotKey);
                if (!File.Exists(path)) { onSuccessJson?.Invoke(string.Empty); return; }
                onSuccessJson?.Invoke(File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex.Message);
            }
        }

        private string GetPath(string slotKey)
        {
            string file = string.IsNullOrWhiteSpace(slotKey) ? "progress_v1" : slotKey;
            return Path.Combine(Application.persistentDataPath, file + ".cloud.json");
        }
    }
}
