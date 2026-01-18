using System.Collections.Generic;
using Game.Application.Ports;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class ChallengesPageControllerBehaviour : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ChallengeDataSO challengeData;

        [Header("Slots (2-3 recommended)")]
        [SerializeField] private List<ChallengeSlot> slots = new();

        [Header("Summary")]
        [SerializeField] private TMP_Text completionSummaryText;
        [SerializeField] private TMP_Text selectedPetText;
        [SerializeField] private TMP_Text selectedOutfitText;

        private IProgressSaveGateway _save;
        private PlayerProgressData _data;
        private ChallengesService _service;

        private void Awake()
        {
            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();
            InitializeService();
            Refresh();
        }

        private void OnEnable()
        {
            if (_save == null)
            {
                _save = new PlayerPrefsProgressSaveGateway();
            }

            _data = _save.Load();
            InitializeService();
            Refresh();
        }

        public void Refresh()
        {
            if (_service == null)
            {
                SetSlotsInactive();
                UpdateSummary();
                UpdateSelections();
                return;
            }

            bool changed = _service.ResetIfNeeded(System.DateTimeOffset.UtcNow);
            changed |= _service.EnsureEntries();

            if (changed)
            {
                _save.Save(_data);
            }

            IReadOnlyList<ChallengeDefinition> definitions = _service.Definitions;
            for (int i = 0; i < slots.Count; i++)
            {
                ChallengeSlot slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                if (i >= definitions.Count)
                {
                    slot.SetActive(false);
                    continue;
                }

                ChallengeDefinition definition = definitions[i];
                if (definition == null)
                {
                    slot.SetActive(false);
                    continue;
                }

                _service.TryGetProgress(definition, out ChallengeProgress entry);
                slot.SetActive(true);
                slot.Apply(definition, entry);
                bool isComplete = entry != null && entry.completed;
                bool isClaimed = entry != null && entry.rewardClaimed;
                slot.SetClaimState(isComplete, isClaimed);
                ChallengeDefinition capturedDefinition = definition;
                slot.BindClaim(() => HandleClaim(capturedDefinition));
            }

            UpdateSummary();
            UpdateSelections();
        }

        private void InitializeService()
        {
            if (challengeData == null)
            {
                _service = null;
                return;
            }

            IReadOnlyList<ChallengeDefinition> definitions = challengeData.BuildDefinitions();
            _service = new ChallengesService(_data, definitions);
        }

        private void SetSlotsInactive()
        {
            foreach (ChallengeSlot slot in slots)
            {
                slot?.SetActive(false);
                slot?.BindClaim(null);
            }
        }

        private void HandleClaim(ChallengeDefinition definition)
        {
            if (_service == null || definition == null)
            {
                return;
            }

            if (_service.TryClaimReward(definition))
            {
                _save.Save(_data);
                Refresh();
            }
        }

        private void UpdateSummary()
        {
            if (completionSummaryText == null || _data == null)
            {
                return;
            }

            int total = 0;
            int completed = 0;
            int claimed = 0;

            if (_data.challengeProgress != null)
            {
                total = _data.challengeProgress.Count;
                foreach (ChallengeProgress entry in _data.challengeProgress)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    if (entry.completed)
                    {
                        completed++;
                    }

                    if (entry.rewardClaimed)
                    {
                        claimed++;
                    }
                }
            }

            completionSummaryText.text = MetaProgressTextFormatter.BuildChallengeSummary(
                completed,
                total,
                claimed);
        }

        private void UpdateSelections()
        {
            if (_data == null)
            {
                return;
            }

            if (selectedPetText != null)
            {
                selectedPetText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected pet", _data.selectedPetId);
            }

            if (selectedOutfitText != null)
            {
                selectedOutfitText.text = MetaProgressTextFormatter.BuildSelectionLabel("Selected outfit", _data.selectedOutfitId);
            }
        }

        [System.Serializable]
        public sealed class ChallengeSlot
        {
            public GameObject root;
            public TMP_Text titleText;
            public TMP_Text descriptionText;
            public TMP_Text progressText;
            public TMP_Text rewardText;
            public Button claimButton;
            public TMP_Text claimButtonText;

            public void SetActive(bool active)
            {
                if (root != null)
                {
                    root.SetActive(active);
                }
            }

            public void Apply(ChallengeDefinition definition, ChallengeProgress entry)
            {
                if (titleText != null)
                {
                    titleText.text = definition.title;
                }

                if (descriptionText != null)
                {
                    descriptionText.text = definition.description;
                }

                if (progressText != null)
                {
                    progressText.text = BuildProgressText(definition, entry);
                }

                if (rewardText != null)
                {
                    rewardText.text = BuildRewardText(definition);
                }
            }

            public void SetClaimState(bool isComplete, bool isClaimed)
            {
                if (claimButton == null)
                {
                    return;
                }

                claimButton.interactable = isComplete && !isClaimed;
                if (claimButtonText != null)
                {
                    if (!isComplete)
                    {
                        claimButtonText.text = "In Progress";
                    }
                    else
                    {
                        claimButtonText.text = isClaimed ? "Claimed" : "Claim";
                    }
                }
            }

            public void BindClaim(System.Action onClaim)
            {
                if (claimButton == null)
                {
                    return;
                }

                claimButton.onClick.RemoveAllListeners();
                if (onClaim != null)
                {
                    claimButton.onClick.AddListener(() => onClaim());
                }
            }

            private static string BuildProgressText(ChallengeDefinition definition, ChallengeProgress entry)
            {
                float progress = entry != null ? entry.current : 0f;
                float target = Mathf.Max(0f, definition.target);
                bool completed = entry != null && entry.completed;

                string unitSuffix = definition.metric == ChallengeMetric.DistanceMeters ? "m" : string.Empty;
                int progressInt = Mathf.FloorToInt(progress);
                int targetInt = Mathf.FloorToInt(target);

                if (completed)
                {
                    return $"Complete ({targetInt}{unitSuffix})";
                }

                if (targetInt <= 0)
                {
                    return $"0{unitSuffix}";
                }

                return $"{progressInt}/{targetInt}{unitSuffix}";
            }

            private static string BuildRewardText(ChallengeDefinition definition)
            {
                var parts = new List<string>();
                if (definition.rewardKibble > 0)
                {
                    parts.Add($"{definition.rewardKibble} Kibble");
                }

                if (definition.rewardGems > 0)
                {
                    parts.Add($"{definition.rewardGems} Gems");
                }

                if (parts.Count == 0)
                {
                    return "Reward: -";
                }

                return $"Reward: {string.Join(", ", parts)}";
            }
        }
    }
}
