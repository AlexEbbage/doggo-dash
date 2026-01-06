using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Run
{
    /// <summary>
    /// Hardened run-state controller: stop runner on fail; restart via scene reload.
    /// </summary>
    public sealed class RunStateControllerBehaviour : MonoBehaviour, IRunFailSink
    {
        [Header("Refs")]
        public MonoBehaviour runnerControllerBehaviour = default!; // RunnerControllerBehaviour
        public bool freezeTimeOnFail = false;

        public bool IsFailed { get; private set; }

        public void OnRunFailed(RunFailReason reason, ObstacleType? obstacleType = null)
        {
            if (IsFailed) return;
            IsFailed = true;

            Debug.Log($"[Run] FAILED: {reason} ({obstacleType?.ToString() ?? "n/a"})");

            if (runnerControllerBehaviour != null)
                runnerControllerBehaviour.enabled = false;

            if (freezeTimeOnFail)
                Time.timeScale = 0f;
        }

        public void RestartScene()
        {
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }
    }
}
