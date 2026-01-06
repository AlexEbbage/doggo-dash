using UnityEngine;
using TMPro;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Run
{
    public sealed class ProgressBankBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public RunStateControllerBehaviour runState = default!;
        public RunRewardTrackerBehaviour runRewards = default!;

        [Header("Optional UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;

        private IProgressSaveGateway _save = default!;
        private PlayerProgressData _data = default!;
        private bool _bankedThisFail;

        public int TotalKibble => _data.totalKibble;
        public int TotalGems => _data.totalGems;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();
            RefreshUI();
        }

        private void Update()
        {
            if (runState == null) return;

            if (!runState.IsFailed)
            {
                _bankedThisFail = false;
                return;
            }

            if (_bankedThisFail) return;
            _bankedThisFail = true;

            if (runRewards != null)
            {
                _data.totalKibble += runRewards.Kibble;
                _data.totalGems += runRewards.Gems;
                _save.Save(_data);
                RefreshUI();
            }
        }

        public void ResetProgressForTesting()
        {
            _data = new PlayerProgressData();
            _save.Save(_data);
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (totalKibbleText != null) totalKibbleText.text = $"{_data.totalKibble}";
            if (totalGemsText != null) totalGemsText.text = $"{_data.totalGems}";
        }
    }
}
