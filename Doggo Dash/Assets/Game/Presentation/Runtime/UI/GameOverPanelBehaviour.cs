using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.UI
{
    public sealed class GameOverPanelBehaviour : MonoBehaviour
    {
        [Header("Refs")]
        public RunStateControllerBehaviour runState = default!;
        public RunResetControllerBehaviour runReset;
        public RunRewardTrackerBehaviour rewards = default!;
        public ScoreDistanceControllerBehaviour scoreDistance = default!;

        [Header("UI")]
        public GameObject panelRoot = default!;
        public TMP_Text summaryText = default!;

        [Header("Scenes")]
        public string hubSceneName = "Hub";

        private bool _shown;

        private void Awake()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (!_shown && runState != null && runState.IsFailed)
                Show();
        }

        private void Show()
        {
            _shown = true;

            if (panelRoot != null)
                panelRoot.SetActive(true);

            if (summaryText != null)
            {
                int meters = scoreDistance != null ? Mathf.FloorToInt(scoreDistance.DistanceMeters) : 0;
                int score = scoreDistance != null ? scoreDistance.Score : 0;
                int kibble = rewards != null ? rewards.Kibble : 0;
                int gems = rewards != null ? rewards.Gems : 0;

                summaryText.text =
                    $"Distance: {meters}m\n" +
                    $"Score: {score}\n" +
                    $"Kibble: {kibble}\n" +
                    $"Gems: {gems}";
            }
        }

        public void Restart()
        {
            if (runState != null)
                runState.RestartScene();
        }

        public void BackToHub()
        {
            if (string.IsNullOrWhiteSpace(hubSceneName))
            {
                Debug.LogWarning("Hub scene name is not set.", this);
                return;
            }

            Time.timeScale = 1f;
            runReset?.ResetRun();
            SceneManager.LoadScene(hubSceneName);
        }
    }
}
