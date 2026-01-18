using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Application.Ports;
using Game.Application.Services;

namespace Game.Infrastructure.Persistence
{
    public sealed class PlayerPrefsProgressSaveGateway : IProgressSaveGateway
    {
        private const string Key = "GAME_PROGRESS_V1";
        private const int DefaultLevel = 1;
        private const int DefaultXpToNext = 100;
        private const float DefaultEnergyMax = 100f;

        public bool TryLoadValidated(out PlayerProgressData data, out bool needsSave)
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrWhiteSpace(json))
            {
                data = CreateDefaultData();
                needsSave = true;
                return true;
            }

            try
            {
                data = JsonUtility.FromJson<PlayerProgressData>(json);
                if (data == null)
                {
                    data = CreateDefaultData();
                    needsSave = true;
                    return false;
                }

                needsSave = ApplyDefaults(data);
                return true;
            }
            catch
            {
                data = CreateDefaultData();
                needsSave = true;
                return false;
            }
        }

        public PlayerProgressData Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrWhiteSpace(json))
                return CreateDefaultData();

            try
            {
                PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json) ?? new PlayerProgressData();
                ApplyDefaults(data);
                ApplyMigrations(data);
                return data;
            }
            catch
            {
                return CreateDefaultData();
            }
        }

        public void Save(PlayerProgressData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }

        private static PlayerProgressData CreateDefaultData()
        {
            var data = new PlayerProgressData();
            ApplyDefaults(data);
            ApplyMigrations(data);
            return data;
        }

        private static bool ApplyDefaults(PlayerProgressData data)
        {
            if (data == null) return false;

            bool changed = false;

            if (data.totalKibble < 0)
            {
                data.totalKibble = 0;
                changed = true;
            }

            if (data.totalGems < 0)
            {
                data.totalGems = 0;
                changed = true;
            }

            if (data.bestScore < 0)
            {
                data.bestScore = 0;
                changed = true;
            }

            if (data.bestDistanceMeters < 0f)
            {
                data.bestDistanceMeters = 0f;
                changed = true;
            }

            if (SanitizeFloat(ref data.bestDistanceMeters, 0f)) changed = true;

            bool xpInitialized = data.level > 0 || data.xp > 0 || data.xpToNext > 0;
            if (!xpInitialized)
            {
                if (data.level != DefaultLevel) changed = true;
                if (data.xp != 0) changed = true;
                if (data.xpToNext != DefaultXpToNext) changed = true;
                data.level = DefaultLevel;
                data.xp = 0;
                data.xpToNext = DefaultXpToNext;
            }
            else
            {
                if (data.level <= 0)
                {
                    data.level = DefaultLevel;
                    changed = true;
                }
                if (data.xpToNext <= 0)
                {
                    data.xpToNext = DefaultXpToNext;
                    changed = true;
                }
            }

            if (SanitizeFloat(ref data.energyMax, DefaultEnergyMax)) changed = true;
            if (SanitizeFloat(ref data.energyCurrent, DefaultEnergyMax)) changed = true;

            bool energyInitialized = data.energyMax > 0f || data.energyCurrent > 0f;
            if (!energyInitialized)
            {
                if (!Mathf.Approximately(data.energyMax, DefaultEnergyMax)) changed = true;
                if (!Mathf.Approximately(data.energyCurrent, DefaultEnergyMax)) changed = true;
                data.energyMax = DefaultEnergyMax;
                data.energyCurrent = data.energyMax;
            }
            else if (data.energyMax <= 0f)
            {
                data.energyMax = DefaultEnergyMax;
                changed = true;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (data.lastEnergyTimestampUtc <= 0)
            {
                data.lastEnergyTimestampUtc = now;
                changed = true;
            }
            if (data.lastXpTimestampUtc <= 0)
            {
                data.lastXpTimestampUtc = now;
                changed = true;
            }

            DateTimeOffset today = DateTimeOffset.UtcNow.Date;
            if (data.lastDailyChallengesResetUtc <= 0)
            {
                data.lastDailyChallengesResetUtc = today.ToUnixTimeSeconds();
                changed = true;
            }

            if (data.lastWeeklyChallengesResetUtc <= 0)
            {
                DateTimeOffset weekStart = GetWeekStartUtc(today);
                data.lastWeeklyChallengesResetUtc = weekStart.ToUnixTimeSeconds();
                changed = true;
            }

            if (data.challengeProgress == null)
            {
                data.challengeProgress = new System.Collections.Generic.List<ChallengeProgressEntry>();
                changed = true;
            }
            else if (SanitizeChallengeProgress(data.challengeProgress))
            {
                changed = true;
            }
            
            if (string.IsNullOrWhiteSpace(data.selectedPetId))
            {
                data.selectedPetId = PlayerProgressData.DefaultPetId;
                changed = true;
            }
            
            if (string.IsNullOrWhiteSpace(data.selectedOutfitId))
            {
                data.selectedOutfitId = PlayerProgressData.DefaultOutfitId;
                changed = true;
            }
 
            if (data.ownedPets == null)
            {
                data.ownedPets = new List<string>();
                changed = true;
            }
            if (data.ownedOutfits == null)
            {
                data.ownedOutfits = new List<string>();
                changed = true;
            }
            if (SanitizeStringList(data.ownedPets)) changed = true;
            if (SanitizeStringList(data.ownedOutfits)) changed = true;
            if (EnsureOwned(data.ownedPets, data.selectedPetId)) changed = true;
            if (EnsureOwned(data.ownedOutfits, data.selectedOutfitId)) changed = true;

            int xpBefore = data.xp;
            int xpToNextBefore = data.xpToNext;
            float energyCurrentBefore = data.energyCurrent;
            float energyMaxBefore = data.energyMax;
            int upgradeEnergyMaxBefore = data.upgradeEnergyMax;
            int upgradeStartSpeedBefore = data.upgradeStartSpeed;
            int upgradeGemBonusBefore = data.upgradeGemBonus;

            ProgressClampUtility.ClampProgress(data);
            if (data.xp != xpBefore) changed = true;
            if (data.xpToNext != xpToNextBefore) changed = true;
            if (!Mathf.Approximately(data.energyCurrent, energyCurrentBefore)) changed = true;
            if (!Mathf.Approximately(data.energyMax, energyMaxBefore)) changed = true;
            if (data.upgradeEnergyMax != upgradeEnergyMaxBefore) changed = true;
            if (data.upgradeStartSpeed != upgradeStartSpeedBefore) changed = true;
            if (data.upgradeGemBonus != upgradeGemBonusBefore) changed = true;

            return changed;
        }

        private static DateTimeOffset GetWeekStartUtc(DateTimeOffset utcDate)
        {
            int diff = (7 + (int)utcDate.DayOfWeek - (int)DayOfWeek.Monday) % 7;
            return utcDate.AddDays(-diff);
        }

        private static void ApplyMigrations(PlayerProgressData data)
        {
            if (data == null) return;

            data.ownedPets ??= new List<string>();
            data.ownedOutfits ??= new List<string>();

            if (string.IsNullOrWhiteSpace(data.selectedPetId))
            {
                data.selectedPetId = PlayerProgressData.DefaultPetId;
            }

            if (string.IsNullOrWhiteSpace(data.selectedOutfitId))
            {
                data.selectedOutfitId = PlayerProgressData.DefaultOutfitId;
            }

            EnsureOwned(data.ownedPets, data.selectedPetId);
            EnsureOwned(data.ownedOutfits, data.selectedOutfitId);
        }

        private static void EnsureOwned(List<string> ownedList, string itemId)
        {
            if (ownedList == null) return false;
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (ownedList.Contains(itemId)) return false;
            ownedList.Add(itemId);
            return true;
        }

        private static bool SanitizeFloat(ref float value, float fallback)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                value = fallback;
                return true;
            }

            return false;
        }

        private static bool SanitizeStringList(List<string> values)
        {
            if (values == null) return false;
            bool changed = false;
            var seen = new HashSet<string>();
            for (int i = values.Count - 1; i >= 0; i--)
            {
                string entry = values[i];
                if (string.IsNullOrWhiteSpace(entry) || !seen.Add(entry))
                {
                    values.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        private static bool SanitizeChallengeProgress(List<ChallengeProgressEntry> entries)
        {
            if (entries == null) return false;
            bool changed = false;
            var seen = new HashSet<string>();
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                ChallengeProgressEntry entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || !seen.Add(entry.id))
                {
                    entries.RemoveAt(i);
                    changed = true;
                    continue;
                }

                if (entry.progress < 0f)
                {
                    entry.progress = 0f;
                    changed = true;
                }

                if (entry.completedUtcSeconds < 0)
                {
                    entry.completedUtcSeconds = 0;
                    changed = true;
                }
            }

            return changed;
        }
    }
}
