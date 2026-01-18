using System;
using System.Collections.Generic;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public sealed class ChallengesService
    {
        private readonly PlayerProgressData _data;
        private readonly List<ChallengeDefinition> _definitions;

        public ChallengesService(PlayerProgressData data, IEnumerable<ChallengeDefinition> definitions)
        {
            _data = data;
            _definitions = definitions == null ? new List<ChallengeDefinition>() : new List<ChallengeDefinition>(definitions);
            _data.challengeProgress ??= new List<ChallengeProgressEntry>();
        }

        public IReadOnlyList<ChallengeDefinition> Definitions => _definitions;

        public bool ResetIfNeeded(DateTimeOffset utcNow)
        {
            bool changed = false;
            DateTimeOffset today = utcNow.Date;
            long todaySeconds = today.ToUnixTimeSeconds();

            if (_data.lastDailyChallengesResetUtc != todaySeconds)
            {
                ResetChallenges(ChallengePeriod.Daily);
                _data.lastDailyChallengesResetUtc = todaySeconds;
                changed = true;
            }

            DateTimeOffset weekStart = GetWeekStartUtc(today);
            long weekSeconds = weekStart.ToUnixTimeSeconds();
            if (_data.lastWeeklyChallengesResetUtc != weekSeconds)
            {
                ResetChallenges(ChallengePeriod.Weekly);
                _data.lastWeeklyChallengesResetUtc = weekSeconds;
                changed = true;
            }

            return changed;
        }

        public bool EnsureEntries()
        {
            bool changed = false;
            foreach (ChallengeDefinition definition in _definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.id))
                {
                    continue;
                }

                if (!TryGetEntry(definition.id, out _))
                {
                    _data.challengeProgress.Add(new ChallengeProgressEntry { id = definition.id });
                    changed = true;
                }
            }

            return changed;
        }

        public bool ApplyRunResults(float distanceMeters, int treatPickups, int gemPickups, int badFoodHits)
        {
            bool changed = false;
            foreach (ChallengeDefinition definition in _definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.id))
                {
                    continue;
                }

                if (!TryGetEntry(definition.id, out ChallengeProgressEntry entry))
                {
                    entry = new ChallengeProgressEntry { id = definition.id };
                    _data.challengeProgress.Add(entry);
                    changed = true;
                }

                if (entry.completed)
                {
                    continue;
                }

                float delta = GetDelta(definition.metric, distanceMeters, treatPickups, gemPickups, badFoodHits);
                if (delta <= 0f)
                {
                    continue;
                }

                entry.progress += delta;
                changed = true;

                if (definition.target > 0f && entry.progress >= definition.target)
                {
                    entry.completed = true;
                    entry.completedUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    GrantRewards(definition);
                    changed = true;
                }
            }

            return changed;
        }

        public bool TryGetProgress(ChallengeDefinition definition, out ChallengeProgressEntry entry)
        {
            entry = null;
            if (definition == null || string.IsNullOrWhiteSpace(definition.id))
            {
                return false;
            }

            return TryGetEntry(definition.id, out entry);
        }

        private bool TryGetEntry(string id, out ChallengeProgressEntry entry)
        {
            entry = null;
            if (string.IsNullOrWhiteSpace(id) || _data.challengeProgress == null)
            {
                return false;
            }

            for (int i = 0; i < _data.challengeProgress.Count; i++)
            {
                ChallengeProgressEntry candidate = _data.challengeProgress[i];
                if (candidate != null && candidate.id == id)
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        private void ResetChallenges(ChallengePeriod period)
        {
            if (_data.challengeProgress == null)
            {
                return;
            }

            HashSet<string> ids = new HashSet<string>();
            foreach (ChallengeDefinition definition in _definitions)
            {
                if (definition == null || definition.period != period)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(definition.id))
                {
                    ids.Add(definition.id);
                }
            }

            foreach (ChallengeProgressEntry entry in _data.challengeProgress)
            {
                if (entry == null)
                {
                    continue;
                }

                if (ids.Contains(entry.id))
                {
                    entry.progress = 0f;
                    entry.completed = false;
                    entry.completedUtcSeconds = 0;
                }
            }
        }

        private static float GetDelta(ChallengeMetric metric, float distanceMeters, int treatPickups, int gemPickups, int badFoodHits)
        {
            switch (metric)
            {
                case ChallengeMetric.DistanceMeters:
                    return Math.Max(0f, distanceMeters);
                case ChallengeMetric.TreatPickups:
                    return Math.Max(0, treatPickups);
                case ChallengeMetric.GemPickups:
                    return Math.Max(0, gemPickups);
                case ChallengeMetric.BadFoodHits:
                    return Math.Max(0, badFoodHits);
                default:
                    return 0f;
            }
        }

        private void GrantRewards(ChallengeDefinition definition)
        {
            if (definition.rewardKibble > 0)
            {
                _data.totalKibble += definition.rewardKibble;
            }

            if (definition.rewardGems > 0)
            {
                _data.totalGems += definition.rewardGems;
            }
        }

        private static DateTimeOffset GetWeekStartUtc(DateTimeOffset utcDate)
        {
            int diff = (7 + (int)utcDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return utcDate.AddDays(-diff);
        }
    }
}
