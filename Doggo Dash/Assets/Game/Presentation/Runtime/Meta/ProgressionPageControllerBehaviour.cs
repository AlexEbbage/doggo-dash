using System;
using Game.Application.Ports;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class ProgressionPageControllerBehaviour : MonoBehaviour
    {
        [Serializable]
        public sealed class UpgradeEntry
        {
            public MetaUpgradeType upgradeType;
            public Button button;
            public TMP_Text titleText;
            public TMP_Text levelText;
            public TMP_Text costText;
            [NonSerialized] public UnityAction cachedAction;
        }

        [Header("Upgrades")]
        [SerializeField] private UpgradeEntry[] upgrades = Array.Empty<UpgradeEntry>();

        [Header("UI")]
        [SerializeField] private TMP_Text totalKibbleText;
        [SerializeField] private TMP_Text totalGemsText;
        [SerializeField] private TMP_Text feedbackText;

        private MetaProgressionService _progression = default!;
        private bool _bound;

        private void Awake()
        {
            _progression = new MetaProgressionService(new PlayerPrefsProgressSaveGateway());
            BindButtons();
            Refresh();
        }

        private void OnEnable()
        {
            BindButtons();
            Refresh();
        }

        private void OnDisable()
        {
            if (upgrades == null) return;
            foreach (UpgradeEntry entry in upgrades)
            {
                if (entry?.button == null || entry.cachedAction == null) continue;
                entry.button.onClick.RemoveListener(entry.cachedAction);
                entry.cachedAction = null;
            }

            _bound = false;
        }

        public void Refresh()
        {
            _progression.Reload();
            PlayerProgressData data = _progression.Data;

            if (totalKibbleText != null) totalKibbleText.text = $"{data.totalKibble}";
            if (totalGemsText != null) totalGemsText.text = $"{data.totalGems}";

            if (upgrades == null) return;
            foreach (UpgradeEntry entry in upgrades)
            {
                RefreshEntry(entry);
            }
        }

        private void BindButtons()
        {
            if (_bound) return;
            if (upgrades == null) return;

            foreach (UpgradeEntry entry in upgrades)
            {
                if (entry == null || entry.button == null) continue;
                MetaUpgradeType upgradeType = entry.upgradeType;
                entry.cachedAction = () => HandlePurchase(upgradeType);
                entry.button.onClick.AddListener(entry.cachedAction);
            }

            _bound = true;
        }

        private void RefreshEntry(UpgradeEntry entry)
        {
            if (entry == null) return;
            MetaUpgradeDefinition def = _progression.GetDefinition(entry.upgradeType);
            int level = _progression.GetUpgradeLevel(entry.upgradeType);
            int cost = _progression.GetUpgradeCost(entry.upgradeType);
            bool maxed = _progression.IsMaxed(entry.upgradeType);

            if (entry.titleText != null)
            {
                entry.titleText.text = def.DisplayName;
            }

            if (entry.levelText != null)
            {
                entry.levelText.text = $"Lv {level}/{def.MaxLevel}";
            }

            if (entry.costText != null)
            {
                entry.costText.text = maxed ? "MAX" : $"Cost: {cost} {def.Currency}";
            }

            if (entry.button != null)
            {
                entry.button.interactable = !maxed;
            }
        }

        private void HandlePurchase(MetaUpgradeType upgradeType)
        {
            MetaUpgradePurchaseResult result = _progression.TryPurchaseUpgrade(upgradeType);
            if (feedbackText != null)
            {
                feedbackText.text = result switch
                {
                    MetaUpgradePurchaseResult.Purchased => "Upgrade purchased!",
                    MetaUpgradePurchaseResult.NotEnoughCurrency => "Not enough currency.",
                    MetaUpgradePurchaseResult.Maxed => "Upgrade already maxed.",
                    _ => "Unable to purchase."
                };
            }

            Refresh();
        }
    }
}
