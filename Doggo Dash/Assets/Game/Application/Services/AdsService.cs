using System;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public sealed class AdsService : IAdsService
    {
        public bool IsRewardedAdReady => false;
        public bool IsAdsRemoved => false;

        public void ShowRewardedAd(Action<bool> onCompleted)
        {
            // TODO: Replace with platform-specific SDK integration once finalized.
            onCompleted?.Invoke(false);
        }

        public void PurchaseRemoveAds(Action<bool> onCompleted)
        {
            // TODO: Replace with platform-specific SDK integration once finalized.
            onCompleted?.Invoke(false);
        }
    }
}
