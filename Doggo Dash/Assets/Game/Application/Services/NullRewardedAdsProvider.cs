using System;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public sealed class NullRewardedAdsProvider : IRewardedAdsProvider
    {
        public bool IsRewardedAdReady => false;

        public void ShowRewardedAd(Action<bool> onCompleted)
        {
            onCompleted?.Invoke(false);
        }
    }
}
