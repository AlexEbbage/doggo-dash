using System;
using System.Collections.Generic;
using Game.Application.Services;
using UnityEngine;

namespace Game.Presentation.Runtime.Meta
{
    [CreateAssetMenu(menuName = "Game/Configs/Challenge Data", fileName = "ChallengeData")]
    public sealed class ChallengeDataSO : ScriptableObject
    {
        public List<ChallengeEntry> challenges = new();

        public IReadOnlyList<ChallengeDefinition> BuildDefinitions()
        {
            if (challenges == null || challenges.Count == 0)
            {
                return Array.Empty<ChallengeDefinition>();
            }

            var definitions = new ChallengeDefinition[challenges.Count];
            for (int i = 0; i < challenges.Count; i++)
            {
                ChallengeEntry entry = challenges[i];
                if (entry == null)
                {
                    definitions[i] = new ChallengeDefinition();
                    continue;
                }

                definitions[i] = new ChallengeDefinition
                {
                    id = entry.id,
                    title = entry.title,
                    description = entry.description,
                    metric = entry.metric,
                    period = entry.period,
                    target = entry.target,
                    rewardKibble = entry.rewardKibble,
                    rewardGems = entry.rewardGems
                };
            }

            return definitions;
        }

        [Serializable]
        public sealed class ChallengeEntry
        {
            public string id = "challenge_id";
            public string title = "Challenge Title";
            [TextArea] public string description = "Complete the objective.";
            public ChallengeMetric metric = ChallengeMetric.DistanceMeters;
            public ChallengePeriod period = ChallengePeriod.Daily;
            public float target = 100f;
            public int rewardKibble = 50;
            public int rewardGems;
        }
    }
}
