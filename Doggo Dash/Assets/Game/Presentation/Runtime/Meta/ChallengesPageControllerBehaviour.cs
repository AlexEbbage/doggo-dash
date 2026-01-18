using System.Collections.Generic;
using Game.Application.Ports;
using Game.Application.Services;
using Game.Infrastructure.Persistence;
using TMPro;
using UnityEngine;

namespace Game.Presentation.Runtime.Meta
{
    public sealed class ChallengesPageControllerBehaviour : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private ChallengeDataSO challengeData;

        [Header("Slots (2-3 recommended)")]
        [SerializeField] private List<ChallengeSlot> slots = new();

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

                _service.TryGetProgress(definition, out ChallengeProgressEntry entry);
                slot.SetActive(true);
                slot.Apply(definition, entry);
            }
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

            public void SetActive(bool active)
            {
                if (root != null)
                {
                    root.SetActive(active);
                }
            }

            public void Apply(ChallengeDefinition definition, ChallengeProgressEntry entry)
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

            private static string BuildProgressText(ChallengeDefinition definition, ChallengeProgressEntry entry)
            {
                float progress = entry != null ? entry.progress : 0f;
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
