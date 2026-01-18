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

        public PlayerProgressData Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrWhiteSpace(json))
                return CreateDefaultData();

            try
            {
                PlayerProgressData data = JsonUtility.FromJson<PlayerProgressData>(json) ?? new PlayerProgressData();
                ApplyDefaults(data);
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

        private static PlayerProgressData CreateDefaultData()
        {
            var data = new PlayerProgressData();
            ApplyDefaults(data);
            return data;
        }

        private static void ApplyDefaults(PlayerProgressData data)
        {
            if (data == null) return;

            bool xpInitialized = data.level > 0 || data.xp > 0 || data.xpToNext > 0;
            if (!xpInitialized)
            {
                data.level = DefaultLevel;
                data.xp = 0;
                data.xpToNext = DefaultXpToNext;
            }
            else
            {
                if (data.level <= 0) data.level = DefaultLevel;
                if (data.xpToNext <= 0) data.xpToNext = DefaultXpToNext;
            }

            bool energyInitialized = data.energyMax > 0f || data.energyCurrent > 0f;
            if (!energyInitialized)
            {
                data.energyMax = DefaultEnergyMax;
                data.energyCurrent = data.energyMax;
            }
            else if (data.energyMax <= 0f)
            {
                data.energyMax = DefaultEnergyMax;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (data.lastEnergyTimestampUtc <= 0) data.lastEnergyTimestampUtc = now;
            if (data.lastXpTimestampUtc <= 0) data.lastXpTimestampUtc = now;

            if (string.IsNullOrWhiteSpace(data.selectedPetId)) data.selectedPetId = "dog_default";
            if (string.IsNullOrWhiteSpace(data.selectedOutfitId)) data.selectedOutfitId = "outfit_default";

            if (data.ownedPets == null) data.ownedPets = new List<string>();
            if (data.ownedOutfits == null) data.ownedOutfits = new List<string>();
            EnsureOwned(data.ownedPets, data.selectedPetId);
            EnsureOwned(data.ownedOutfits, data.selectedOutfitId);

            ProgressClampUtility.ClampProgress(data);
        }

        private static void EnsureOwned(List<string> ownedList, string itemId)
        {
            if (ownedList == null) return;
            if (string.IsNullOrWhiteSpace(itemId)) return;
            if (!ownedList.Contains(itemId)) ownedList.Add(itemId);
        }
    }
}
