using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.IAP
{
    public sealed class IapServiceBehaviour : MonoBehaviour
    {
        public IapCatalogSO catalog = default!;
        public MonoBehaviour providerBehaviour = default!;
        public bool log = true;

        private IIapProvider _provider = default!;
        private IProgressSaveGateway _save = default!;
        private PlayerProgressData _data = default!;

        private void Awake()
        {
            _provider = providerBehaviour as IIapProvider;
            if (_provider == null) { Debug.LogError("[IAP] providerBehaviour must implement IIapProvider."); enabled = false; return; }
            if (catalog == null) { Debug.LogError("[IAP] catalog not assigned."); enabled = false; return; }

            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();

            _provider.Initialize(
                onReady: () => { if (log) Debug.Log("[IAP] Initialized"); },
                onError: err => Debug.LogWarning($"[IAP] Init error: {err}")
            );
        }

        public void Purchase(IapProductId id)
        {
            if (!_provider.IsInitialized) { Debug.LogWarning("[IAP] Not initialized yet."); return; }
            if (!catalog.TryGet(id, out var entry)) { Debug.LogError($"[IAP] Missing catalog entry for {id}"); return; }

            _provider.Purchase(
                id,
                onSuccess: () => { Grant(entry); if (log) Debug.Log($"[IAP] Purchase success: {id}"); },
                onError: err => Debug.LogWarning($"[IAP] Purchase failed: {id} - {err}")
            );
        }

        public void Restore()
        {
            if (!_provider.IsInitialized) { Debug.LogWarning("[IAP] Not initialized yet."); return; }

            _provider.RestorePurchases(
                onComplete: () => { if (log) Debug.Log("[IAP] Restore complete"); },
                onError: err => Debug.LogWarning($"[IAP] Restore error: {err}")
            );
        }

        private void Grant(IapCatalogSO.Entry entry)
        {
            _data = _save.Load();

            if (entry.grantGems > 0) _data.totalGems += entry.grantGems;
            if (entry.grantsRemoveAds) _data.removeAdsOwned = true;

            _save.Save(_data);
        }
    }
}
