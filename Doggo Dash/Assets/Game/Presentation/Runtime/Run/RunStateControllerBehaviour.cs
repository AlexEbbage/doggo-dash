using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Application.Ports;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.FX;

namespace Game.Presentation.Runtime.Run
{
    public sealed class RunStateControllerBehaviour : MonoBehaviour, IRunFailSink
    {
        [Header("Refs")]
        public MonoBehaviour runnerControllerBehaviour = default!;
        public MonoBehaviour collisionReporterBehaviour = default!;
        public RunResetControllerBehaviour runReset;

        [Header("Feedback (optional)")]
        public RunFeedbackControllerBehaviour feedback;

        [Header("Debug")]
        public bool freezeTimeOnFail = false;

        public bool IsFailed { get; private set; }

        private void Awake()
        {
            if (feedback == null)
                feedback = FindObjectOfType<RunFeedbackControllerBehaviour>();
        }

        public void OnRunFailed(RunFailReason reason, ObstacleType? obstacleType = null)
        {
            if (IsFailed) return;
            IsFailed = true;

            Debug.Log($"[Run] FAILED: {reason} ({obstacleType?.ToString() ?? "n/a"})");

            if (runnerControllerBehaviour != null)
                runnerControllerBehaviour.enabled = false;

            feedback?.PlayGameOver();

            if (freezeTimeOnFail)
                Time.timeScale = 0f;
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            runReset?.ResetRun();
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        [ContextMenu("DEV/Force Fail")]
        private void DevForceFail()
        {
            OnRunFailed(RunFailReason.ObstacleHit, ObstacleType.FullBlock);
        }
    }
}
