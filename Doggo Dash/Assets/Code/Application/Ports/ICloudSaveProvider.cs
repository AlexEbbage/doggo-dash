using System;

namespace Game.Application.Ports
{
    public interface ICloudSaveProvider
    {
        bool IsReady { get; }
        void Initialize(Action onReady, Action<string> onError);

        void Upload(string slotKey, string json, Action onSuccess, Action<string> onError);
        void Download(string slotKey, Action<string> onSuccessJson, Action<string> onError);

        string ProviderName { get; }
    }
}
