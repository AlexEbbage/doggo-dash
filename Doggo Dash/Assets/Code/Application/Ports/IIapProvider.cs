using System;
using Game.Domain.ValueObjects;

namespace Game.Application.Ports
{
    public interface IIapProvider
    {
        bool IsInitialized { get; }
        void Initialize(Action onReady, Action<string> onError);

        void Purchase(IapProductId productId, Action onSuccess, Action<string> onError);
        void RestorePurchases(Action onComplete, Action<string> onError);
    }
}
