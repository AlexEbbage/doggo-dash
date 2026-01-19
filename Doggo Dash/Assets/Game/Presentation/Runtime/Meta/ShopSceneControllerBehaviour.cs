using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;
using Game.Infrastructure.Persistence;
using Game.Application.Ports;
using Game.Application.Services;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class ShopSceneControllerBehaviour : MonoBehaviour
    {
        private const string HubSceneName = "Hub";

        [Header("Scenes")]
        [FormerlySerializedAs("menuSceneName")]
        public string hubSceneName = HubSceneName;

        [Header("Catalog")]
        public ShopCatalogSO catalog = default!;

        [Header("UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;

        public TMP_Text selectedPetText;
        public TMP_Text selectedOutfitText;

        public TMP_Text feedbackText;
        public TMP_Text buyButtonText;

        [Header("IAP")]
        public MonoBehaviour iapServiceBehaviour;

        private MetaProgressService _progress = default!;
        private IIapService _iapService = default!;
        private int _index;

        private void Awake()
        {
            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            _iapService = ResolveIapService();
            NormalizeSceneNames();
            _index = 0;
            RefreshAll();
            ShowCurrentItem();
        }

        private void OnEnable()
        {
            NormalizeSceneNames();
            RefreshAll();
            ShowCurrentItem();
        }

        private void RefreshAll()
        {
            if (_progress == null)
            {
                _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            }

            _progress.Reload();
            var d = _progress.Data;

            if (totalKibbleText != null) totalKibbleText.text = $"{d.totalKibble}";
            if (totalGemsText != null) totalGemsText.text = $"{d.totalGems}";

            if (selectedPetText != null)
            {
                selectedPetText.text = MetaProgressTextFormatter.BuildSelectionLabel("Pet", d.selectedPetId);
            }

            if (selectedOutfitText != null)
            {
                selectedOutfitText.text = MetaProgressTextFormatter.BuildSelectionLabel("Outfit", d.selectedOutfitId);
            }
        }

        private ShopItemSO CurrentItem
        {
            get
            {
                if (catalog == null || catalog.items == null || catalog.items.Length == 0) return null;
                _index = Mathf.Clamp(_index, 0, catalog.items.Length - 1);
                return catalog.items[_index];
            }
        }

        public void NextItem()
        {
            if (catalog == null || catalog.items == null || catalog.items.Length == 0) return;
            _index = (_index + 1) % catalog.items.Length;
            ShowCurrentItem();
        }

        public void PrevItem()
        {
            if (catalog == null || catalog.items == null || catalog.items.Length == 0) return;
            _index = (_index - 1 + catalog.items.Length) % catalog.items.Length;
            ShowCurrentItem();
        }

        public void ShowCurrentItem()
        {
            var item = CurrentItem;
            if (feedbackText == null) return;

            if (item == null)
            {
                feedbackText.text = "No items in catalog.";
                return;
            }

            bool isOwned = item.type != ShopItemType.GemPack && _progress.IsOwned(item.type, item.itemId);
            if (buyButtonText != null)
            {
                buyButtonText.text = GetBuyButtonLabel(item, isOwned);
            }

            feedbackText.text =
                $"{item.displayName}\n" +
                $"{item.type}\n" +
                $"{BuildItemDetails(item, isOwned)}";
        }

        public void BuyOrSelectCurrent()
        {
            var item = CurrentItem;
            if (item == null)
            {
                SetFeedback("No item selected.");
                return;
            }

            if (item.type == ShopItemType.GemPack)
            {
                HandleGemPackPurchase(item);
                return;
            }

            bool isOwned = _progress.IsOwned(item.type, item.itemId);
            if (!isOwned)
            {
                bool paid = item.price <= 0 || TrySpend(item.currency, item.price);
                if (!paid) return;
                _progress.GrantOwnership(item.type, item.itemId);
            }

            if (item.type == ShopItemType.Pet)
            {
                _progress.SetSelectedPet(item.itemId);
                SetFeedback($"Selected pet: {item.displayName}");
            }
            else
            {
                _progress.SetSelectedOutfit(item.itemId);
                SetFeedback($"Selected outfit: {item.displayName}");
            }

            RefreshAll();
            ShowCurrentItem();
        }

        private void HandleGemPackPurchase(ShopItemSO item)
        {
            if (!string.IsNullOrWhiteSpace(item.iapProductId))
            {
                if (!_iapService.CanPurchase(item.iapProductId))
                {
                    SetFeedback("Purchases unavailable.");
                    return;
                }

                _iapService.Purchase(item.iapProductId, success =>
                {
                    if (!success)
                    {
                        SetFeedback("Purchase canceled.");
                        return;
                    }

                    GrantGemPack(item);
                });
                return;
            }

            if (item.currency == ShopCurrency.Gems)
            {
                SetFeedback("Gem packs cannot be purchased with gems.");
                return;
            }

            bool paid = item.price <= 0 || TrySpend(item.currency, item.price);
            if (!paid) return;

            GrantGemPack(item);
        }

        private void GrantGemPack(ShopItemSO item)
        {
            int gemAmount = Mathf.Max(0, item.gemAmount);
            _progress.AddGems(gemAmount);
            SetFeedback(gemAmount > 0
                ? $"Purchased {gemAmount} gems!"
                : "Purchased gem pack.");

            RefreshAll();
            ShowCurrentItem();
        }

        private static string BuildItemDetails(ShopItemSO item, bool isOwned)
        {
            if (item.type == ShopItemType.GemPack)
            {
                string amountText = item.gemAmount > 0 ? $"Includes {item.gemAmount} gems\n" : string.Empty;
                if (!string.IsNullOrWhiteSpace(item.iapProductId))
                {
                    return $"{amountText}Cost: IAP";
                }

                return $"{amountText}Cost: {item.price} {item.currency}";
            }

            return isOwned ? "Owned" : $"Cost: {item.price} {item.currency}";
        }

        private string GetBuyButtonLabel(ShopItemSO item, bool isOwned)
        {
            if (item == null) return "Buy";
            if (item.type == ShopItemType.GemPack) return "Buy";
            if (!isOwned) return "Buy";

            bool isSelected = item.type switch
            {
                ShopItemType.Pet => _progress.Data.selectedPetId == item.itemId,
                ShopItemType.Outfit => _progress.Data.selectedOutfitId == item.itemId,
                _ => false
            };

            return isSelected ? "Owned" : "Equip";
        }

        private bool TrySpend(ShopCurrency currency, int price)
        {
            bool ok = currency switch
            {
                ShopCurrency.Kibble => _progress.SpendKibble(price),
                ShopCurrency.Gems => _progress.SpendGems(price),
                _ => false
            };

            if (!ok)
            {
                SetFeedback($"Not enough {currency}.");
                return false;
            }

            return true;
        }

        private void SetFeedback(string msg)
        {
            if (feedbackText != null)
                feedbackText.text = msg;
        }

        public void BackToMenu()
        {
            NormalizeSceneNames();
            SceneManager.LoadScene(hubSceneName);
        }

        private void OnValidate()
        {
            NormalizeSceneNames();
        }

        private void NormalizeSceneNames()
        {
            if (string.IsNullOrWhiteSpace(hubSceneName) || hubSceneName == "Menu" || hubSceneName == "Shop")
            {
                hubSceneName = HubSceneName;
            }
        }

        private IIapService ResolveIapService()
        {
            if (iapServiceBehaviour != null && iapServiceBehaviour is IIapService service)
            {
                return service;
            }

            return new IapService();
        }
    }
}
