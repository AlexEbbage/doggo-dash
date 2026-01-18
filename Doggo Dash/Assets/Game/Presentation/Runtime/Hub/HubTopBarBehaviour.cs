using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Hub
{
    public sealed class HubTopBarBehaviour : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Slider xpBar;

        [Header("XP")]
        [Min(1)]
        [SerializeField] private int fallbackXpToNext = 100;

        private IProgressSaveGateway _save = default!;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
        }

        private void OnEnable()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (_save == null)
            {
                _save = new PlayerPrefsProgressSaveGateway();
            }

            PlayerProgressData data = _save.Load();
            int level = data.level > 0 ? data.level : 1;
            int xpToNext = data.xpToNext > 0 ? data.xpToNext : Mathf.Max(1, fallbackXpToNext);
            int xpValue = Mathf.Clamp(data.xp, 0, xpToNext);

            if (levelText != null)
            {
                levelText.text = $"Lvl {level}";
            }

            if (xpBar != null)
            {
                xpBar.minValue = 0f;
                xpBar.maxValue = xpToNext;
                xpBar.value = xpValue;
            }
        }
    }
}
