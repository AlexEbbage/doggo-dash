using System;

namespace Game.Application.Ports
{
    public interface IAdsService
    {
        bool IsRewardedAdReady { get; }
        bool IsAdsRemoved { get; }
        void ShowRewardedAd(Action<bool> onCompleted);
        void PurchaseRemoveAds(Action<bool> onCompleted);
    }
}
