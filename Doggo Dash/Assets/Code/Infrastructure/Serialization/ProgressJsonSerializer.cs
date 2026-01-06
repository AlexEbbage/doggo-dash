using UnityEngine;
using Game.Application.Ports;

namespace Game.Infrastructure.Serialization
{
    public static class ProgressJsonSerializer
    {
        public static string ToJson(PlayerProgressData data) => JsonUtility.ToJson(data);

        public static bool TryFromJson(string json, out PlayerProgressData data)
        {
            data = null;
            if (string.IsNullOrWhiteSpace(json)) return false;

            try
            {
                data = JsonUtility.FromJson<PlayerProgressData>(json);
                return data != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
