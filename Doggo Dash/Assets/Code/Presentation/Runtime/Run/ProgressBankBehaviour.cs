using UnityEngine;
using TMPro;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Run
{
    public sealed class ProgressBankBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public RunRewardTrackerBehaviour runRewards = default!;
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        [Header("Optional UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;
        public TMP_Text bestScoreText;
        public TMP_Text bestDistanceText;

        private IProgressSaveGateway _save;
        private PlayerProgressData _data;
        private bool _bankedForThisGameOver;

        public PlayerProgressData Data => _data;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();
            RefreshUI();
        }

        public void ResetForNewRun()
        {
            _bankedForThisGameOver = false;
        }

        public bool BankNow(int rewardMultiplier)
        {
            if (_bankedForThisGameOver) return false;
            _bankedForThisGameOver = true;

            rewardMultiplier = Mathf.Max(1, rewardMultiplier);

            _data = _save.Load();

            if (runRewards != null)
            {
                _data.totalKibble += runRewards.Kibble * rewardMultiplier;
                _data.totalGems += runRewards.Gems * rewardMultiplier;
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
            return true;
        }

        public void ForceReload()
        {
            _data = _save.Load();
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
