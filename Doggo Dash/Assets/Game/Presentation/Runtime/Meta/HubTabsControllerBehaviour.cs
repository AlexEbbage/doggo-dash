using System.Collections;
using System.Collections.Generic;
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

        [Header("Tab Visuals")]
        public Vector3 selectedScale = new Vector3(1.05f, 1.05f, 1.05f);
        public Vector3 unselectedScale = Vector3.one;
        public Color selectedColor = Color.white;
        public Color unselectedColor = new Color(1f, 1f, 1f, 0.6f);

        [Header("Page Transitions (optional)")]
        public bool enablePageTransitions = true;
        public float pageFadeDuration = 0.2f;
        public Vector2 pageSlideOffset = new Vector2(40f, 0f);

        private HubTab _current = HubTab.Shop;
        private readonly Dictionary<GameObject, Vector2> _pageAnchors = new Dictionary<GameObject, Vector2>();
        private readonly Dictionary<GameObject, Coroutine> _pageTransitions = new Dictionary<GameObject, Coroutine>();

        private void Awake()
        {
            // Wire button callbacks (no inspector OnClick needed)
            if (tabShop) tabShop.onClick.AddListener(() => Show(HubTab.Shop));
            if (tabCharacters) tabCharacters.onClick.AddListener(() => Show(HubTab.Characters));
            if (tabPlay) tabPlay.onClick.AddListener(() => Show(HubTab.Play));
            if (tabChallenges) tabChallenges.onClick.AddListener(() => Show(HubTab.Challenges));
            if (tabProgression) tabProgression.onClick.AddListener(() => Show(HubTab.Progression));

            CachePageTransform(pageShop);
            CachePageTransform(pageCharacters);
            CachePageTransform(pagePlay);
            CachePageTransform(pageChallenges);
            CachePageTransform(pageProgression);

            Show(_current);
        }

        public void Show(HubTab tab)
        {
            _current = tab;

            SetPageActive(pageShop, tab == HubTab.Shop);
            SetPageActive(pageCharacters, tab == HubTab.Characters);
            SetPageActive(pagePlay, tab == HubTab.Play);
            SetPageActive(pageChallenges, tab == HubTab.Challenges);
            SetPageActive(pageProgression, tab == HubTab.Progression);

            SetSelected(tabShop, tab == HubTab.Shop);
            SetSelected(tabCharacters, tab == HubTab.Characters);
            SetSelected(tabPlay, tab == HubTab.Play);
            SetSelected(tabChallenges, tab == HubTab.Challenges);
            SetSelected(tabProgression, tab == HubTab.Progression);
        }

        public void StartRun()
        {
            SceneManager.LoadScene(gameSceneName);
        }

        public void SetSelected(Button button, bool selected)
        {
            if (button == null)
            {
                return;
            }

            button.transform.localScale = selected ? selectedScale : unselectedScale;
            Color targetColor = selected ? selectedColor : unselectedColor;

            foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
            {
                graphic.color = targetColor;
            }
        }

        private void CachePageTransform(GameObject page)
        {
            if (page == null)
            {
                return;
            }

            RectTransform rectTransform = page.GetComponent<RectTransform>();
            if (rectTransform == null || _pageAnchors.ContainsKey(page))
            {
                return;
            }

            _pageAnchors[page] = rectTransform.anchoredPosition;
        }

        private void SetPageActive(GameObject page, bool isActive)
        {
            if (page == null)
            {
                return;
            }

            if (_pageTransitions.TryGetValue(page, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
                _pageTransitions.Remove(page);
            }

            CanvasGroup canvasGroup = page.GetComponent<CanvasGroup>();
            RectTransform rectTransform = page.GetComponent<RectTransform>();

            if (!isActive)
            {
                if (!enablePageTransitions || canvasGroup == null || rectTransform == null || !page.activeSelf)
                {
                    page.SetActive(false);
                    return;
                }

                Vector2 anchor = GetOrCacheAnchor(page, rectTransform);
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                _pageTransitions[page] = StartCoroutine(AnimatePageOut(page, canvasGroup, rectTransform, anchor));
                return;
            }

            page.SetActive(true);

            if (!enablePageTransitions || canvasGroup == null || rectTransform == null)
            {
                return;
            }

            Vector2 activeAnchor = GetOrCacheAnchor(page, rectTransform);
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            rectTransform.anchoredPosition = activeAnchor + pageSlideOffset;

            _pageTransitions[page] = StartCoroutine(AnimatePageIn(page, canvasGroup, rectTransform, activeAnchor));
        }

        private Vector2 GetOrCacheAnchor(GameObject page, RectTransform rectTransform)
        {
            if (_pageAnchors.TryGetValue(page, out Vector2 anchor))
            {
                return anchor;
            }

            anchor = rectTransform.anchoredPosition;
            _pageAnchors[page] = anchor;
            return anchor;
        }

        private IEnumerator AnimatePageIn(GameObject page, CanvasGroup canvasGroup, RectTransform rectTransform, Vector2 targetPosition)
        {
            float duration = Mathf.Max(0.01f, pageFadeDuration);
            float elapsed = 0f;
            Vector2 startPosition = rectTransform.anchoredPosition;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            rectTransform.anchoredPosition = targetPosition;
            _pageTransitions.Remove(page);
        }

        private IEnumerator AnimatePageOut(GameObject page, CanvasGroup canvasGroup, RectTransform rectTransform, Vector2 anchorPosition)
        {
            float duration = Mathf.Max(0.01f, pageFadeDuration);
            float elapsed = 0f;
            Vector2 startPosition = rectTransform.anchoredPosition;
            Vector2 targetPosition = anchorPosition - pageSlideOffset;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            rectTransform.anchoredPosition = targetPosition;
            page.SetActive(false);
            _pageTransitions.Remove(page);
        }
    }
}
