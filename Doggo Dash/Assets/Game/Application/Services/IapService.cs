using System;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public sealed class IapService : IIapService
    {
        public bool IsInitialized => true;

        public bool CanPurchase(string productId)
        {
            return !string.IsNullOrWhiteSpace(productId);
        }

        public void Purchase(string productId, Action<bool> onCompleted)
        {
            if (!CanPurchase(productId))
            {
                onCompleted?.Invoke(false);
                return;
            }

            // TODO: Replace with platform-specific IAP SDK integration once finalized.
            onCompleted?.Invoke(true);
        }
    }
}
