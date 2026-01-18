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
        [SerializeField] private TMP_Text selectedPetText;
        [SerializeField] private TMP_Text selectedOutfitText;
        [SerializeField] private TMP_Text challengeSummaryText;

        private MetaProgressionService _progression = default!;
        private bool _bound;

        private void Awake()
        {
            _progression = new MetaProgressionService(new PlayerPrefsProgressSaveGateway());
            EnsureFallbackSetup();
            BindButtons();
            Refresh();
        }

        private void OnEnable()
        {
            EnsureFallbackSetup();
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
            if (selectedPetText != null)
            {
                selectedPetText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected pet", data.selectedPetId);
            }

            if (selectedOutfitText != null)
            {
                selectedOutfitText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected outfit", data.selectedOutfitId);
            }

            if (challengeSummaryText != null)
            {
                challengeSummaryText.text = MetaProgressTextFormatter.BuildChallengeSummary(data);
            }

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

        private void EnsureFallbackSetup()
        {
            if (upgrades != null && upgrades.Length > 0) return;

            Button[] buttons = GetComponentsInChildren<Button>(true);
            if (buttons == null || buttons.Length == 0) return;

            MetaUpgradeType[] order =
            {
                MetaUpgradeType.EnergyMax,
                MetaUpgradeType.StartSpeed,
                MetaUpgradeType.GemBonus
            };

            int count = Mathf.Min(buttons.Length, order.Length);
            upgrades = new UpgradeEntry[count];
            for (int i = 0; i < count; i++)
            {
                Button button = buttons[i];
                UpgradeEntry entry = new UpgradeEntry
                {
                    upgradeType = order[i],
                    button = button
                };

                EnsureEntryText(entry, button.transform);
                upgrades[i] = entry;
            }

            if (feedbackText == null)
            {
                feedbackText = CreateTextElement(transform, "UpgradeFeedbackText", 18f, TextAlignmentOptions.Center);
                if (feedbackText != null) feedbackText.text = string.Empty;
            }
        }

        private void EnsureEntryText(UpgradeEntry entry, Transform parent)
        {
            if (entry == null || parent == null) return;

            Transform textGroup = parent.Find("UpgradeTextGroup");
            if (textGroup == null)
            {
                textGroup = CreateTextGroup(parent);
            }

            entry.titleText ??= FindOrCreateText(textGroup, "TitleText", 18f);
            entry.levelText ??= FindOrCreateText(textGroup, "LevelText", 14f);
            entry.costText ??= FindOrCreateText(textGroup, "CostText", 14f);
        }

        private static Transform CreateTextGroup(Transform parent)
        {
            var groupObject = new GameObject("UpgradeTextGroup", typeof(RectTransform));
            RectTransform rectTransform = groupObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.offsetMin = new Vector2(6f, 6f);
            rectTransform.offsetMax = new Vector2(-6f, -6f);

            VerticalLayoutGroup layout = groupObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 2f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = groupObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            return rectTransform;
        }

        private static TMP_Text FindOrCreateText(Transform parent, string name, float fontSize)
        {
            if (parent == null) return null;
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                TMP_Text existingText = existing.GetComponent<TMP_Text>();
                if (existingText != null) return existingText;
            }

            return CreateTextElement(parent, name, fontSize, TextAlignmentOptions.Center);
        }

        private static TMP_Text CreateTextElement(Transform parent, string name, float fontSize, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = string.Empty;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;

            return text;
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
