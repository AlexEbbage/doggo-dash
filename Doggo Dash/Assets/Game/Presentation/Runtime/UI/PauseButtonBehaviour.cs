using UnityEngine;

namespace Game.Presentation.Runtime.UI
{
    public sealed class PauseButtonBehaviour : MonoBehaviour
    {
        public GameObject pausePanel;
        private bool _paused;

        public void TogglePause()
        {
            _paused = !_paused;
            Time.timeScale = _paused ? 0f : 1f;
            if (pausePanel != null) pausePanel.SetActive(_paused);
        }

        public void Resume()
        {
            _paused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
        }
    }
}
