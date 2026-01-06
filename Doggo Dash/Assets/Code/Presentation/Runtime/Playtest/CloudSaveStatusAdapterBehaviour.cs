using UnityEngine;
using Game.Presentation.Runtime.CloudSave;

namespace Game.Presentation.Runtime.Playtest
{
    public sealed class CloudSaveStatusAdapterBehaviour : MonoBehaviour, ICloudSaveStatusSource
    {
        public CloudSaveServiceBehaviour cloud = default!;

        public string ProviderName => cloud != null ? cloud.ProviderNameSafe : "none";
        public string SlotKey => cloud != null ? cloud.SlotKeySafe : "";
        public string LastStatus => cloud != null ? cloud.LastStatusSafe : "";
        public long LastSyncUnixUtc => cloud != null ? cloud.LastSyncUnixUtcSafe : 0;
    }
}
