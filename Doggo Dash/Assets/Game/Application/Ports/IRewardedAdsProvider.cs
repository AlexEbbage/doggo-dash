using System;

namespace Game.Application.Ports
{
    public interface IRewardedAdsProvider
    {
        bool IsRewardedAdReady { get; }
        void ShowRewardedAd(Action<bool> onCompleted);
    }
}
