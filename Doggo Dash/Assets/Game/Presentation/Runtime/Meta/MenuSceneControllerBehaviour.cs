using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using TMPro;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class MenuSceneControllerBehaviour : MonoBehaviour
    {
        private const string HubSceneName = "Hub";
        private const string GameSceneName = "Game";

        [Header("Scenes")]
        public string gameSceneName = GameSceneName;

        [FormerlySerializedAs("shopSceneName")]
        public string hubSceneName = HubSceneName;

        [Header("UI")]
        public TMP_Text totalKibbleText;
        public TMP_Text totalGemsText;
        public TMP_Text bestScoreText;
        public TMP_Text bestDistanceText;

        private MetaProgressService _progress = default!;

        private void Awake()
        {
            _progress = new MetaProgressService(new PlayerPrefsProgressSaveGateway());
            NormalizeSceneNames();
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

        public void Play()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                Debug.LogWarning("Game scene name is not set.", this);
                return;
            }

            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenShop()
        {
            NormalizeSceneNames();
            SceneManager.LoadScene(hubSceneName);
        }

        public void Quit() => UnityEngine.Application.Quit();

        private void OnValidate()
        {
            NormalizeSceneNames();
        }

        private void NormalizeSceneNames()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
            {
                gameSceneName = GameSceneName;
            }

            if (string.IsNullOrWhiteSpace(hubSceneName) || hubSceneName == "Menu" || hubSceneName == "Shop")
            {
                hubSceneName = HubSceneName;
            }
        }
    }
}
