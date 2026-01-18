using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class HubTopBarBehaviour : MonoBehaviour
    {
        [Header("Currency")]
        [SerializeField] private TMP_Text kibbleText;
        [SerializeField] private Image kibbleIcon;
        [SerializeField] private TMP_Text gemsText;
        [SerializeField] private Image gemsIcon;

        [Header("Progression")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text xpText;
        [SerializeField] private Slider xpBar;
        [SerializeField] private int xpPerLevel = 1000;

        [Header("Avatar")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private GameObject avatarPlaceholder;

        [Header("Selections")]
        [SerializeField] private TMP_Text selectedPetText;
        [SerializeField] private TMP_Text selectedOutfitText;

        [Header("Settings")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;

        private MetaProgressService _progress = default!;
        private bool _settingsBound;
        private bool _referencesCached;
        private const int MinXpPerLevel = 1;

        private void Awake()
        {
            EnsureProgressService();
            CacheReferences();
            BindSettingsButton();
            Refresh();
        }

        private void Reset()
        {
            CacheReferences(force: true);
        }

        private void OnEnable()
        {
            EnsureProgressService();
            CacheReferences();
            BindSettingsButton();
            Refresh();
        }

        private void OnDisable()
        {
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            }

            _settingsBound = false;
        }

        public void Refresh()
        {
            if (_progress == null)
            {
                return;
            }

            _progress.Reload();
            PlayerProgressData data = _progress.Data;

            if (kibbleText != null)
            {
                kibbleText.text = $"{data.totalKibble}";
            }

            if (gemsText != null)
            {
                gemsText.text = $"{data.totalGems}";
            }

            int perLevel = Mathf.Max(MinXpPerLevel, xpPerLevel);
            int totalXp = Mathf.Max(0, data.bestScore);
            int level = Mathf.Max(1, (totalXp / perLevel) + 1);
            int xpIntoLevel = totalXp % perLevel;
            float progress = xpIntoLevel / (float)perLevel;

            if (levelText != null)
            {
                levelText.text = $"Lvl {level}";
            }

            if (xpText != null)
            {
                xpText.text = $"{xpIntoLevel}/{perLevel} XP";
            }

            if (xpBar != null)
            {
                xpBar.minValue = 0f;
                xpBar.maxValue = 1f;
                xpBar.normalizedValue = progress;
            }

            if (avatarImage != null && avatarPlaceholder != null)
            {
                avatarPlaceholder.SetActive(avatarImage.sprite == null);
            }

            if (selectedPetText != null)
            {
                selectedPetText.text = $"Pet: {data.selectedPetId}";
            }

            if (selectedOutfitText != null)
            {
                selectedOutfitText.text = $"Outfit: {data.selectedOutfitId}";
            }
        }

        private void EnsureProgressService()
        {
            if (_progress != null)
            {
                return;
            }

            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
        }

        private void BindSettingsButton()
        {
            if (settingsButton == null)
            {
                return;
            }

            if (_settingsBound)
            {
                return;
            }

            settingsButton.onClick.AddListener(HandleSettingsClicked);
            _settingsBound = true;
        }

        private void HandleSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
                return;
            }

            Debug.Log("Settings clicked (placeholder)");
        }

        private void CacheReferences(bool force = false)
        {
            if (_referencesCached && !force)
            {
                return;
            }

            if (levelText == null)
            {
                levelText = FindTextByName("LevelText");
            }

            if (xpBar == null)
            {
                Transform xpTransform = FindChildByName("XpBar");
                if (xpTransform != null)
                {
                    xpBar = xpTransform.GetComponent<Slider>();
                }
            }

            if (settingsButton == null)
            {
                Transform settingsTransform = FindChildByName("SettingsButton");
                if (settingsTransform != null)
                {
                    settingsButton = settingsTransform.GetComponent<Button>();
                }
            }

            if (avatarImage == null)
            {
                Transform avatarTransform = FindChildByName("Avatar");
                if (avatarTransform != null)
                {
                    avatarImage = avatarTransform.GetComponent<Image>();
                }
            }

            if (kibbleText == null || kibbleIcon == null)
            {
                CacheCurrency("SoftCurrencyPill", ref kibbleText, ref kibbleIcon);
            }

            if (gemsText == null || gemsIcon == null)
            {
                CacheCurrency("PremiumCurrencyPill", ref gemsText, ref gemsIcon);
            }

            if (xpText == null)
            {
                xpText = FindTextByName("XpText");
            }

            if (selectedPetText == null)
            {
                selectedPetText = FindTextByName("SelectedPetText");
            }

            if (selectedOutfitText == null)
            {
                selectedOutfitText = FindTextByName("SelectedOutfitText");
            }

            _referencesCached = kibbleText != null
                && kibbleIcon != null
                && gemsText != null
                && gemsIcon != null
                && levelText != null
                && xpBar != null;
        }

        private void CacheCurrency(string rootName, ref TMP_Text valueText, ref Image iconImage)
        {
            Transform root = FindChildByName(rootName);
            if (root == null)
            {
                return;
            }

            if (valueText == null)
            {
                Transform valueTransform = FindChildByName("ValueText", root);
                if (valueTransform != null)
                {
                    valueText = valueTransform.GetComponent<TMP_Text>();
                }
            }

            if (iconImage == null)
            {
                Transform iconTransform = FindChildByName("Icon", root);
                if (iconTransform != null)
                {
                    iconImage = iconTransform.GetComponent<Image>();
                }
            }
        }

        private Transform FindChildByName(string childName, Transform searchRoot = null)
        {
            Transform root = searchRoot != null ? searchRoot : transform;
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName)
                {
                    return child;
                }
            }

            return null;
        }

        private TMP_Text FindTextByName(string childName)
        {
            Transform child = FindChildByName(childName);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<TMP_Text>();
        }
    }
}
