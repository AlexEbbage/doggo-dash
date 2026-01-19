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

        [Header("Energy")]
        [SerializeField] private TMP_Text energyText;

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
        private Sprite _defaultSprite;
        private Sprite _checkmarkSprite;

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

            int level = Mathf.Max(1, data.level);
            int xpCurrent = Mathf.Max(0, data.xp);
            int xpTarget = Mathf.Max(1, data.xpToNext);
            float progress = xpCurrent / (float)xpTarget;

            if (levelText != null)
            {
                levelText.text = $"Lvl {level}";
            }

            if (xpText != null)
            {
                xpText.text = $"{xpCurrent}/{xpTarget} XP";
            }

            if (xpBar != null)
            {
                xpBar.minValue = 0f;
                xpBar.maxValue = 1f;
                xpBar.normalizedValue = progress;
            }

            if (energyText != null)
            {
                int energyCurrent = Mathf.RoundToInt(Mathf.Max(0f, data.energyCurrent));
                int energyMax = Mathf.RoundToInt(Mathf.Max(0f, data.energyMax));
                energyText.text = $"{energyCurrent}/{energyMax}";
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
            OpenSettingsOverlay();
        }

        public void OpenSettingsOverlay()
        {
            if (settingsPanel == null)
            {
                settingsPanel = CreateSettingsPanel();
            }

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
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

            if (energyText == null)
            {
                energyText = FindTextByName("EnergyText");
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

        private GameObject CreateSettingsPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            Transform parent = canvas != null ? canvas.transform : transform;
            GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.SetParent(parent, false);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 360f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.85f);

            TMP_Text title = CreateText(panel.transform, "Title", "Settings", new Vector2(0f, 135f), new Vector2(360f, 40f), 36);
            title.alignment = TextAlignmentOptions.Center;

            TMP_Text soundLabel = CreateText(panel.transform, "SoundLabel", "Sound", new Vector2(-110f, 70f), new Vector2(200f, 30f), 28);
            soundLabel.alignment = TextAlignmentOptions.MidlineLeft;
            Toggle soundToggle = CreateToggle(panel.transform, "SoundToggle", new Vector2(150f, 70f));

            TMP_Text musicLabel = CreateText(panel.transform, "MusicLabel", "Music", new Vector2(-110f, 20f), new Vector2(200f, 30f), 28);
            musicLabel.alignment = TextAlignmentOptions.MidlineLeft;
            Toggle musicToggle = CreateToggle(panel.transform, "MusicToggle", new Vector2(150f, 20f));

            Button privacyButton = CreateButton(panel.transform, "PrivacyButton", new Vector2(0f, -60f), new Vector2(300f, 44f), "Privacy & Terms");
            TMP_Text privacyLabel = privacyButton.GetComponentInChildren<TMP_Text>();

            Button closeButton = CreateButton(panel.transform, "CloseButton", new Vector2(0f, -125f), new Vector2(200f, 40f), "Close");

            HubSettingsPanelBehaviour panelBehaviour = panel.AddComponent<HubSettingsPanelBehaviour>();
            panelBehaviour.Configure(soundToggle, musicToggle, privacyButton, closeButton, privacyLabel);

            return panel;
        }

        private TMP_Text CreateText(Transform parent, string name, string value, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }

            return text;
        }

        private Toggle CreateToggle(Transform parent, string name, Vector2 anchoredPosition)
        {
            GameObject toggleObject = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            RectTransform rectTransform = toggleObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(52f, 28f);
            rectTransform.anchoredPosition = anchoredPosition;

            GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.SetParent(toggleObject.transform, false);
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            Image backgroundImage = backgroundObject.GetComponent<Image>();
            backgroundImage.sprite = GetDefaultSprite();
            backgroundImage.color = new Color(1f, 1f, 1f, 0.2f);

            GameObject checkmarkObject = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            RectTransform checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            checkmarkRect.SetParent(backgroundObject.transform, false);
            checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
            checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
            checkmarkRect.sizeDelta = Vector2.zero;
            Image checkmarkImage = checkmarkObject.GetComponent<Image>();
            checkmarkImage.sprite = GetCheckmarkSprite();
            checkmarkImage.color = new Color(0.2f, 0.9f, 0.4f, 1f);

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;

            return toggle;
        }

        private Button CreateButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, string label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;

            Image image = buttonObject.GetComponent<Image>();
            image.sprite = GetDefaultSprite();
            image.color = new Color(1f, 1f, 1f, 0.15f);

            Button button = buttonObject.GetComponent<Button>();
            TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, size, 24);
            RectTransform labelRect = labelText.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.sizeDelta = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;

            return button;
        }

        private Sprite GetDefaultSprite()
        {
            if (_defaultSprite == null)
            {
                _defaultSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            }

            return _defaultSprite;
        }

        private Sprite GetCheckmarkSprite()
        {
            if (_checkmarkSprite == null)
            {
                _checkmarkSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Checkmark.psd");
            }

            return _checkmarkSprite;
        }
    }
}
