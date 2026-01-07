using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class ShopSceneControllerBehaviour : MonoBehaviour
    {
        [Header("Scenes")]
        public string menuSceneName = "Menu";

        [Header("Catalog")]
        public ShopCatalogSO catalog = default!;

        [Header("UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;

        public TMP_Text selectedPetText;
        public TMP_Text selectedOutfitText;

        public TMP_Text feedbackText;

        private MetaProgressService _progress = default!;
        private int _index;

        private void Awake()
        {
            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            _index = 0;
            RefreshAll();
            ShowCurrentItem();
        }

        private void RefreshAll()
        {
            _progress.Reload();
            var d = _progress.Data;

            if (totalKibbleText != null) totalKibbleText.text = $"{d.totalKibble}";
            if (totalGemsText != null) totalGemsText.text = $"{d.totalGems}";

            if (selectedPetText != null) selectedPetText.text = $"Pet: {d.selectedPetId}";
            if (selectedOutfitText != null) selectedOutfitText.text = $"Outfit: {d.selectedOutfitId}";
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

            feedbackText.text =
                $"{item.displayName}\n" +
                $"{item.type}\n" +
                $"Cost: {item.price} {item.currency}";
        }

        public void BuyOrSelectCurrent()
        {
            var item = CurrentItem;
            if (item == null)
            {
                SetFeedback("No item selected.");
                return;
            }

            bool paid = item.price <= 0 || TrySpend(item.currency, item.price);
            if (!paid) return;

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

        public void BackToMenu() => SceneManager.LoadScene(menuSceneName);
    }
}
