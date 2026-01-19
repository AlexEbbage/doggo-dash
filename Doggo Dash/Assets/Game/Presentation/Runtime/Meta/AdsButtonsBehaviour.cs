using Game.Application.Ports;
using Game.Application.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class AdsButtonsBehaviour : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button rewardedButton;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private TMP_Text feedbackText;

        [Header("Rewarded Ad")]
        [SerializeField] private int rewardedGemAmount = 25;

        [Header("IAP")]
        [SerializeField] private string removeAdsProductId = "remove_ads";
        [SerializeField] private MonoBehaviour iapServiceBehaviour;
        [SerializeField] private MonoBehaviour rewardedAdsProviderBehaviour;

        private IAdsService _adsService = default!;
        private MetaProgressService _progress = default!;

        private void Awake()
        {
            var saveGateway = new PlayerPrefsProgressSaveGateway();
            var iapService = ResolveIapService();
            var rewardedProvider = ResolveRewardedAdsProvider();
            _adsService = new AdsService(saveGateway, iapService, rewardedProvider, removeAdsProductId);
            _progress = new MetaProgressService(saveGateway);
            WireButtons();
            RefreshButtons();
        }

        private void OnEnable()
        {
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (rewardedButton != null) rewardedButton.onClick.RemoveListener(RequestRewardedAd);
            if (removeAdsButton != null) removeAdsButton.onClick.RemoveListener(RequestRemoveAds);
        }

        private void WireButtons()
        {
            if (rewardedButton != null) rewardedButton.onClick.AddListener(RequestRewardedAd);
            if (removeAdsButton != null) removeAdsButton.onClick.AddListener(RequestRemoveAds);
        }

        public void RequestRewardedAd()
        {
            if (!_adsService.IsRewardedAdReady)
            {
                SetFeedback("Rewarded ads coming soon.");
                return;
            }

            _adsService.ShowRewardedAd(success =>
            {
                if (success)
                {
                    if (rewardedGemAmount > 0)
                    {
                        _progress.AddGems(rewardedGemAmount);
                        SetFeedback($"Rewarded ad completed. +{rewardedGemAmount} gems!");
                    }
                    else
                    {
                        SetFeedback("Rewarded ad completed.");
                    }
                }
                else
                {
                    SetFeedback("Rewarded ad skipped.");
                }

                RefreshButtons();
            });
        }

        public void RequestRemoveAds()
        {
            if (_adsService.IsAdsRemoved)
            {
                SetFeedback("Ads already removed.");
                return;
            }

            _adsService.PurchaseRemoveAds(success =>
            {
                SetFeedback(success ? "Ads removed." : "Remove ads coming soon.");
                RefreshButtons();
            });
        }

        private void RefreshButtons()
        {
            if (rewardedButton != null)
                rewardedButton.interactable = _adsService.IsRewardedAdReady;

            if (removeAdsButton != null)
                removeAdsButton.interactable = !_adsService.IsAdsRemoved;
        }

        private void SetFeedback(string message)
        {
            if (feedbackText != null)
                feedbackText.text = message;
        }

        private IIapService ResolveIapService()
        {
            if (iapServiceBehaviour != null && iapServiceBehaviour is IIapService service)
            {
                return service;
            }

            return new IapService();
        }

        private IRewardedAdsProvider ResolveRewardedAdsProvider()
        {
            if (rewardedAdsProviderBehaviour != null && rewardedAdsProviderBehaviour is IRewardedAdsProvider provider)
            {
                return provider;
            }

            return new NullRewardedAdsProvider();
        }
    }
}
