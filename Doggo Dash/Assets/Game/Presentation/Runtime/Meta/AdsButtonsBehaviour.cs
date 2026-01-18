using Game.Application.Ports;
using Game.Application.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class AdsButtonsBehaviour : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button rewardedButton;
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private TMP_Text feedbackText;

        private IAdsService _adsService = default!;

        private void Awake()
        {
            _adsService = new AdsService();
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
                SetFeedback(success ? "Rewarded ad completed." : "Rewarded ad skipped.");
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
    }
}
