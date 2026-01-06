using UnityEngine;
using Game.Application.Ports;

namespace Game.Infrastructure.Persistence
{
    public sealed class PlayerPrefsProgressSaveGateway : IProgressSaveGateway
    {
        private const string Key = "GAME_PROGRESS_V1";

        public PlayerProgressData Load()
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrWhiteSpace(json))
                return new PlayerProgressData();

            try
            {
                return JsonUtility.FromJson<PlayerProgressData>(json) ?? new PlayerProgressData();
            }
            catch
            {
                return new PlayerProgressData();
            }
        }

        public void Save(PlayerProgressData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }
    }
}
