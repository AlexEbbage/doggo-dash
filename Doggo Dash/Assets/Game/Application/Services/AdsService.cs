using System;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Application.Services
{
    public sealed class AdsService : IAdsService
    {
        private const string DefaultRemoveAdsProductId = "remove_ads";

        private readonly IProgressSaveGateway _saveGateway;
        private readonly IIapService _iapService;
        private readonly IRewardedAdsProvider _rewardedAdsProvider;
        private readonly string _removeAdsProductId;

        public AdsService()
            : this(new PlayerPrefsProgressSaveGateway(), new IapService(), new NullRewardedAdsProvider(), DefaultRemoveAdsProductId)
        {
        }

        public AdsService(
            IProgressSaveGateway saveGateway,
            IIapService iapService,
            IRewardedAdsProvider rewardedAdsProvider,
            string removeAdsProductId)
        {
            _saveGateway = saveGateway;
            _iapService = iapService;
            _rewardedAdsProvider = rewardedAdsProvider;
            _removeAdsProductId = string.IsNullOrWhiteSpace(removeAdsProductId)
                ? DefaultRemoveAdsProductId
                : removeAdsProductId;
        }

        public bool IsRewardedAdReady => _rewardedAdsProvider.IsRewardedAdReady;
        public bool IsAdsRemoved => LoadProgress().adRemoved;

        public void ShowRewardedAd(Action<bool> onCompleted)
        {
            _rewardedAdsProvider.ShowRewardedAd(onCompleted);
        }

        public void PurchaseRemoveAds(Action<bool> onCompleted)
        {
            var progress = LoadProgress();
            if (progress.adRemoved)
            {
                onCompleted?.Invoke(true);
                return;
            }

            if (!_iapService.CanPurchase(_removeAdsProductId))
            {
                onCompleted?.Invoke(false);
                return;
            }

            _iapService.Purchase(_removeAdsProductId, success =>
            {
                if (success)
                {
                    progress.adRemoved = true;
                    _saveGateway.Save(progress);
                }

                onCompleted?.Invoke(success);
            });
        }

        private PlayerProgressData LoadProgress()
        {
            return _saveGateway.Load();
        }
    }
}
