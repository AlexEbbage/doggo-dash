using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Game.Presentation.Runtime.FX;

namespace Game.Presentation.Runtime.UI
{
    public sealed class UiAudioClickHandlerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public SfxId clickSfx = SfxId.UiClick;

        [Header("Refs")]
        public AudioPlayerBehaviour audioPlayer;
        public Transform root;

        private readonly List<Button> _buttons = new List<Button>();

        private void Awake()
        {
            if (audioPlayer == null)
                audioPlayer = FindObjectOfType<AudioPlayerBehaviour>();

            RefreshButtons();
        }

        private void OnEnable()
        {
            RegisterButtons();
        }

        private void OnDisable()
        {
            UnregisterButtons();
        }

        public void RefreshButtons()
        {
            UnregisterButtons();
            _buttons.Clear();

            Transform searchRoot = root != null ? root : transform;
            searchRoot.GetComponentsInChildren(true, _buttons);

            RegisterButtons();
        }

        private void RegisterButtons()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                Button button = _buttons[i];
                if (button == null) continue;
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }
        }

        private void UnregisterButtons()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                Button button = _buttons[i];
                if (button == null) continue;
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            audioPlayer?.Play(clickSfx);
        }
    }
}
