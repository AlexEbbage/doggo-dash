using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class MenuSceneControllerBehaviour : MonoBehaviour
    {
        [Header("Scenes")]
        public string gameSceneName = "Game";
        public string shopSceneName = "Hub";

        [Header("UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;
        public TMP_Text bestScoreText;
        public TMP_Text bestDistanceText;

        private MetaProgressService _progress = default!;

        private void Awake()
        {
            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            Refresh();
        }

        private void OnEnable() => Refresh();

        public void Refresh()
        {
            _progress.Reload();
            PlayerProgressData d = _progress.Data;

            if (totalKibbleText != null) totalKibbleText.text = $"{d.totalKibble}";
            if (totalGemsText != null) totalGemsText.text = $"{d.totalGems}";
            if (bestScoreText != null) bestScoreText.text = $"{d.bestScore}";
            if (bestDistanceText != null) bestDistanceText.text = $"{Mathf.FloorToInt(d.bestDistanceMeters)}m";
        }

        public void Play() => SceneManager.LoadScene(gameSceneName);
        public void OpenShop() => SceneManager.LoadScene(shopSceneName);
        public void Quit() => UnityEngine.Application.Quit();
    }
}
