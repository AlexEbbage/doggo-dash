using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Meta
{
    public enum HubTab
    {
        Shop,
        Characters,
        Play,
        Challenges,
        Progression
    }

    public sealed class HubTabsControllerBehaviour : MonoBehaviour
    {
        [Header("Pages")]
        public GameObject pageShop = default!;
        public GameObject pageCharacters = default!;
        public GameObject pagePlay = default!;
        public GameObject pageChallenges = default!;
        public GameObject pageProgression = default!;

        [Header("Tab Buttons (optional visuals later)")]
        public Button tabShop = default!;
        public Button tabCharacters = default!;
        public Button tabPlay = default!;
        public Button tabChallenges = default!;
        public Button tabProgression = default!;

        [Header("Play")]
        public string gameSceneName = "Game";

        private HubTab _current = HubTab.Shop;

        private void Awake()
        {
            // Wire button callbacks (no inspector OnClick needed)
            if (tabShop) tabShop.onClick.AddListener(() => Show(HubTab.Shop));
            if (tabCharacters) tabCharacters.onClick.AddListener(() => Show(HubTab.Characters));
            if (tabPlay) tabPlay.onClick.AddListener(() => Show(HubTab.Play));
            if (tabChallenges) tabChallenges.onClick.AddListener(() => Show(HubTab.Challenges));
            if (tabProgression) tabProgression.onClick.AddListener(() => Show(HubTab.Progression));

            Show(_current);
        }

        public void Show(HubTab tab)
        {
            _current = tab;

            if (pageShop) pageShop.SetActive(tab == HubTab.Shop);
            if (pageCharacters) pageCharacters.SetActive(tab == HubTab.Characters);
            if (pagePlay) pagePlay.SetActive(tab == HubTab.Play);
            if (pageChallenges) pageChallenges.SetActive(tab == HubTab.Challenges);
            if (pageProgression) pageProgression.SetActive(tab == HubTab.Progression);

            // Later: animate transitions + selected tab visuals here.
        }

        public void StartRun()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
