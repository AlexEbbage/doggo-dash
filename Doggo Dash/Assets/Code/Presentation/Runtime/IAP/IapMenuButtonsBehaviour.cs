using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.IAP
{
    public sealed class IapMenuButtonsBehaviour : MonoBehaviour
    {
        public IapServiceBehaviour iap = default!;

        public void BuyGemsSmall() => iap?.Purchase(IapProductId.GemsSmall);
        public void BuyGemsMedium() => iap?.Purchase(IapProductId.GemsMedium);
        public void BuyGemsLarge() => iap?.Purchase(IapProductId.GemsLarge);
        public void BuyRemoveAds() => iap?.Purchase(IapProductId.RemoveAds);
        public void RestorePurchases() => iap?.Restore();
    }
}
