using System;
using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.IAP
{
    public sealed class StubIapProviderBehaviour : MonoBehaviour, IIapProvider
    {
        public bool IsInitialized { get; private set; }
        public bool simulateFailure = false;

        public void Initialize(Action onReady, Action<string> onError)
        {
            IsInitialized = true;
            onReady?.Invoke();
        }

        public void Purchase(IapProductId productId, Action onSuccess, Action<string> onError)
        {
            if (!IsInitialized) { onError?.Invoke("IAP not initialized"); return; }
            if (simulateFailure) { onError?.Invoke("Simulated purchase failure"); return; }
            onSuccess?.Invoke();
        }

        public void RestorePurchases(Action onComplete, Action<string> onError)
        {
            if (!IsInitialized) { onError?.Invoke("IAP not initialized"); return; }
            onComplete?.Invoke();
        }
    }
}
