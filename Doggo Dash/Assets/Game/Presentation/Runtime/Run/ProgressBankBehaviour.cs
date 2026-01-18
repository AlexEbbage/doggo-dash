using UnityEngine;
using TMPro;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using Game.Application.Services;
using Game.Presentation.Runtime.Meta;

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

        [Header("Challenges")]
        public ChallengeDataSO challengeData;

        private IProgressSaveGateway _save = default!;
        private PlayerProgressData _data = default!;
        private ChallengesService _challenges;
        private bool _bankedThisFail;

        public PlayerProgressData Data => _data;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();
            InitializeChallenges();
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

            if (_challenges != null)
            {
                float distance = scoreDistance != null ? scoreDistance.DistanceMeters : 0f;
                int treats = runRewards != null ? runRewards.Kibble : 0;
                int gems = runRewards != null ? runRewards.Gems : 0;
                int badFoodHits = runRewards != null ? runRewards.BadFoodHits : 0;
                _challenges.ResetIfNeeded(System.DateTimeOffset.UtcNow);
                _challenges.ApplyRunResults(distance, treats, gems, badFoodHits);
            }

            _save.Save(_data);
            RefreshUI();
        }

        public void ForceReload()
        {
            _data = _save.Load();
            InitializeChallenges();
            RefreshUI();
        }

        public void ResetProgressForTesting()
        {
            _data = new PlayerProgressData();
            _save.Save(_data);
            InitializeChallenges();
            RefreshUI();
        }

        private void InitializeChallenges()
        {
            if (challengeData == null)
            {
                _challenges = null;
                return;
            }

            _challenges = new ChallengesService(_data, challengeData.BuildDefinitions());
            if (_challenges.ResetIfNeeded(System.DateTimeOffset.UtcNow) | _challenges.EnsureEntries())
            {
                _save.Save(_data);
            }
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
