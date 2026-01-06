using UnityEngine;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Ads
{
    public sealed class GameOverClaimControllerBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public RunStateControllerBehaviour runState = default!;
        public ProgressBankBehaviour bank = default!;
        public RewardedAdsServiceBehaviour ads = default!;

        [Header("UI (optional)")]
        public GameObject gameOverPanelRoot;
        public GameObject doubleOfferRoot;

        private bool _shown;

        private void Awake()
        {
            if (gameOverPanelRoot != null) gameOverPanelRoot.SetActive(false);
            if (doubleOfferRoot != null) doubleOfferRoot.SetActive(false);
        }

        private void Update()
        {
            if (_shown) return;
            if (runState == null) return;

            if (runState.IsFailed)
            {
                _shown = true;

                if (gameOverPanelRoot != null) gameOverPanelRoot.SetActive(true);

                bool canDouble = ads != null && ads.CanShow(AdPlacementId.RewardedDoubleRewards);
                if (doubleOfferRoot != null) doubleOfferRoot.SetActive(canDouble);
            }
        }

        public void ClaimStandard()
        {
            bank?.BankNow(1);
            if (doubleOfferRoot != null) doubleOfferRoot.SetActive(false);
        }

        public void WatchDoubleAndClaim()
        {
            if (ads == null || bank == null) return;

            ads.Show(
                AdPlacementId.RewardedDoubleRewards,
                onRewarded: () =>
                {
                    bank.BankNow(2);
                    if (doubleOfferRoot != null) doubleOfferRoot.SetActive(false);
                },
                onClosedOrFailed: () => { }
            );
        }

        public void ResetRun()
        {
            _shown = false;
            bank?.ResetForNewRun();
            if (gameOverPanelRoot != null) gameOverPanelRoot.SetActive(false);
            if (doubleOfferRoot != null) doubleOfferRoot.SetActive(false);
        }
    }
}
