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
            long dailyReset = today.ToUnixTimeSeconds();
            long weeklyReset = GetWeekStartUtc(today).ToUnixTimeSeconds();

            foreach (ChallengeDefinition definition in _definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.id))
                {
                    continue;
                }

                if (!TryGetEntry(definition.id, out ChallengeProgressEntry entry))
                {
                    entry = CreateEntry(definition);
                    _data.challengeProgress.Add(entry);
                    changed = true;
                }

                long resetAnchor = definition.period == ChallengePeriod.Weekly ? weeklyReset : dailyReset;
                if (entry.lastResetUtcSeconds == 0)
                {
                    changed |= MigrateLegacyEntry(entry, definition, resetAnchor, utcNow);
                }
                else if (entry.lastResetUtcSeconds != resetAnchor)
                {
                    ResetEntry(entry, resetAnchor);
                    changed = true;
                }

                changed |= SyncEntryWithDefinition(entry, definition);

                if (definition.target > 0f && entry.current >= definition.target && !entry.completed)
                {
                    entry.completed = true;
                    if (entry.completedUtcSeconds <= 0)
                    {
                        entry.completedUtcSeconds = utcNow.ToUnixTimeSeconds();
                    }
                    changed = true;
                }
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

                if (!TryGetEntry(definition.id, out ChallengeProgressEntry entry))
                {
                    _data.challengeProgress.Add(CreateEntry(definition));
                    changed = true;
                    continue;
                }

                if (SyncEntryWithDefinition(entry, definition))
                {
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
                    entry = CreateEntry(definition);
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

                float current = entry.current;
                if (current <= 0f && entry.progress > 0f)
                {
                    current = entry.progress;
                }

                current += delta;
                entry.current = current;
                entry.progress = entry.current;
                changed = true;

                if (definition.target > 0f && entry.current >= definition.target)
                {
                    entry.completed = true;
                    entry.completedUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

        public bool TryClaimReward(ChallengeDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.id))
            {
                return false;
            }

            if (!TryGetEntry(definition.id, out ChallengeProgressEntry entry))
            {
                return false;
            }

            if (!entry.completed || entry.rewardClaimed)
            {
                return false;
            }

            entry.rewardClaimed = true;
            GrantRewards(definition);
            return true;
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

        private static ChallengeProgressEntry CreateEntry(ChallengeDefinition definition)
        {
            var entry = new ChallengeProgressEntry { id = definition.id };
            SyncEntryWithDefinition(entry, definition);
            return entry;
        }

        private static bool SyncEntryWithDefinition(ChallengeProgressEntry entry, ChallengeDefinition definition)
        {
            if (entry == null || definition == null)
            {
                return false;
            }

            bool changed = false;
            if (entry.type != definition.metric)
            {
                entry.type = definition.metric;
                changed = true;
            }

            if (Math.Abs(entry.target - definition.target) > 0.001f)
            {
                entry.target = definition.target;
                changed = true;
            }

            if (entry.current <= 0f && entry.progress > 0f)
            {
                entry.current = entry.progress;
                changed = true;
            }

            if (Math.Abs(entry.progress - entry.current) > 0.001f)
            {
                entry.progress = entry.current;
                changed = true;
            }

            return changed;
        }

        private static void ResetEntry(ChallengeProgressEntry entry, long resetAnchorSeconds)
        {
            if (entry == null)
            {
                return;
            }

            entry.current = 0f;
            entry.progress = 0f;
            entry.completed = false;
            entry.rewardClaimed = false;
            entry.completedUtcSeconds = 0;
            entry.lastResetUtcSeconds = resetAnchorSeconds;
        }

        private static bool MigrateLegacyEntry(ChallengeProgressEntry entry, ChallengeDefinition definition, long resetAnchor, DateTimeOffset utcNow)
        {
            if (entry == null || definition == null)
            {
                return false;
            }

            bool changed = false;
            if (entry.current <= 0f && entry.progress > 0f)
            {
                entry.current = entry.progress;
                changed = true;
            }

            if (entry.current < 0f)
            {
                entry.current = 0f;
                changed = true;
            }

            if (definition.target > 0f && entry.current >= definition.target && !entry.completed)
            {
                entry.completed = true;
                if (entry.completedUtcSeconds <= 0)
                {
                    entry.completedUtcSeconds = utcNow.ToUnixTimeSeconds();
                }
                changed = true;
            }

            if (entry.completed && !entry.rewardClaimed)
            {
                entry.rewardClaimed = true;
                changed = true;
            }

            if (entry.lastResetUtcSeconds != resetAnchor)
            {
                entry.lastResetUtcSeconds = resetAnchor;
                changed = true;
            }

            return changed;
        }
    }
}
