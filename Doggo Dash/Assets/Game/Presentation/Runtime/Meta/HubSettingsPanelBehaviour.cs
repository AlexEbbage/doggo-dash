using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class HubSettingsPanelBehaviour : MonoBehaviour
    {
        private const string SoundEnabledKey = "settings.sound.enabled";
        private const string MusicEnabledKey = "settings.music.enabled";

        [Header("Toggles")]
        [SerializeField] private Toggle soundToggle;
        [SerializeField] private Toggle musicToggle;

        [Header("Buttons")]
        [SerializeField] private Button privacyButton;
        [SerializeField] private Button closeButton;

        [Header("Copy")]
        [SerializeField] private TMP_Text privacyLabel;
        [SerializeField] private string privacyTermsUrl = "https://example.com/privacy";

        private bool _bound;

        public void Configure(Toggle sound, Toggle music, Button privacy, Button close, TMP_Text privacyText)
        {
            soundToggle = sound;
            musicToggle = music;
            privacyButton = privacy;
            closeButton = close;
            privacyLabel = privacyText;
        }

        private void OnEnable()
        {
            Bind();
            Load();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void Bind()
        {
            if (_bound)
            {
                return;
            }

            if (soundToggle != null)
            {
                soundToggle.onValueChanged.AddListener(HandleSoundChanged);
            }

            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(HandleMusicChanged);
            }

            if (privacyButton != null)
            {
                privacyButton.onClick.AddListener(HandlePrivacyClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(HandleCloseClicked);
            }

            _bound = true;
        }

        private void Unbind()
        {
            if (!_bound)
            {
                return;
            }

            if (soundToggle != null)
            {
                soundToggle.onValueChanged.RemoveListener(HandleSoundChanged);
            }

            if (musicToggle != null)
            {
                musicToggle.onValueChanged.RemoveListener(HandleMusicChanged);
            }

            if (privacyButton != null)
            {
                privacyButton.onClick.RemoveListener(HandlePrivacyClicked);
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }

            _bound = false;
        }

        private void Load()
        {
            bool soundEnabled = PlayerPrefs.GetInt(SoundEnabledKey, 1) == 1;
            bool musicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;

            if (soundToggle != null)
            {
                soundToggle.SetIsOnWithoutNotify(soundEnabled);
            }

            if (musicToggle != null)
            {
                musicToggle.SetIsOnWithoutNotify(musicEnabled);
            }

            if (privacyLabel != null)
            {
                privacyLabel.text = "Privacy & Terms";
            }
        }

        private void HandleSoundChanged(bool value)
        {
            PlayerPrefs.SetInt(SoundEnabledKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void HandleMusicChanged(bool value)
        {
            PlayerPrefs.SetInt(MusicEnabledKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void HandlePrivacyClicked()
        {
            if (!string.IsNullOrWhiteSpace(privacyTermsUrl))
            {
                UnityEngine.Application.OpenURL(privacyTermsUrl);
                return;
            }

            Debug.Log("Privacy & Terms link placeholder.");
        }

        private void HandleCloseClicked()
        {
            gameObject.SetActive(false);
        }
    }
}
