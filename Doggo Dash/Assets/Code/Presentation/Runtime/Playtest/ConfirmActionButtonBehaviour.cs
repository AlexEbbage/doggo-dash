using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.Presentation.Runtime.Playtest
{
    public sealed class ConfirmActionButtonBehaviour : MonoBehaviour
    {
        public Button button = default!;
        public TMP_Text label;

        [Header("Confirm")]
        public string normalText = "Reset Progress";
        public string confirmText = "Tap again to confirm";
        public float confirmWindowSeconds = 3f;

        [Header("After Confirm")]
        public float disableSecondsAfterConfirm = 1.5f;

        public UnityEngine.Events.UnityEvent onConfirmed;

        private float _t;
        private bool _armed;
        private float _disableT;

        private void Awake()
        {
            if (button == null) button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
            SetLabel(normalText);
        }

        private void Update()
        {
            if (_disableT > 0f)
            {
                _disableT -= Time.unscaledDeltaTime;
                if (_disableT <= 0f && button != null) button.interactable = true;
            }

            if (!_armed) return;

            _t += Time.unscaledDeltaTime;
            if (_t >= confirmWindowSeconds)
            {
                _armed = false;
                _t = 0f;
                SetLabel(normalText);
            }
        }

        private void OnClick()
        {
            if (button != null && !button.interactable) return;

            if (!_armed)
            {
                _armed = true;
                _t = 0f;
                SetLabel(confirmText);
                return;
            }

            _armed = false;
            _t = 0f;
            SetLabel(normalText);

            if (button != null)
            {
                button.interactable = false;
                _disableT = Mathf.Max(0f, disableSecondsAfterConfirm);
            }

            onConfirmed?.Invoke();
        }

        private void SetLabel(string txt)
        {
            if (label != null) label.text = txt;
        }
    }
}
