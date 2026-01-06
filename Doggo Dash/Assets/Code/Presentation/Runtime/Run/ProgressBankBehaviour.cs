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
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        [Header("Optional UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;
        public TMP_Text bestScoreText;
        public TMP_Text bestDistanceText;

        private IProgressSaveGateway _save = default!;
        private PlayerProgressData _data = default!;
        private bool _bankedThisFail;

        public PlayerProgressData Data => _data;

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
            }

            if (scoreDistance != null)
            {
                int score = scoreDistance.Score;
                float dist = scoreDistance.DistanceMeters;

                if (score > _data.bestScore) _data.bestScore = score;
                if (dist > _data.bestDistanceMeters) _data.bestDistanceMeters = dist;
            }

            _save.Save(_data);
            RefreshUI();
        }

        public void ForceReload()
        {
            _data = _save.Load();
            RefreshUI();
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
            if (bestScoreText != null) bestScoreText.text = $"{_data.bestScore}";
            if (bestDistanceText != null) bestDistanceText.text = $"{Mathf.FloorToInt(_data.bestDistanceMeters)}m";
        }
    }
}
