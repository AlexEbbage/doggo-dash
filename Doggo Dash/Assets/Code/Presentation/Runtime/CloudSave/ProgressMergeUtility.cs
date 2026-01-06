using System;
using System.Collections.Generic;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.CloudSave
{
    public static class ProgressMergeUtility
    {
        public static PlayerProgressData Merge(PlayerProgressData local, PlayerProgressData cloud)
        {
            if (local == null) return cloud;
            if (cloud == null) return local;

            bool cloudNewer = IsCloudNewer(local, cloud);

            PlayerProgressData result = cloudNewer ? cloud : local;
            PlayerProgressData other  = cloudNewer ? local : cloud;

            result.totalKibble = Math.Max(result.totalKibble, other.totalKibble);
            result.totalGems   = Math.Max(result.totalGems, other.totalGems);

            result.bestScore = Math.Max(result.bestScore, other.bestScore);
            result.bestDistanceMeters = Math.Max(result.bestDistanceMeters, other.bestDistanceMeters);

            result.removeAdsOwned = result.removeAdsOwned || other.removeAdsOwned;

            result.ownedItemIds = UnionStrings(result.ownedItemIds, other.ownedItemIds);

            if (!string.IsNullOrWhiteSpace(result.lastEventId) && result.lastEventId == other.lastEventId)
                result.eventCurrency = Math.Max(result.eventCurrency, other.eventCurrency);

            result.missionMultiplierLevel = Math.Max(result.missionMultiplierLevel, other.missionMultiplierLevel);

            result.progressRevision = Math.Max(result.progressRevision, other.progressRevision);
            result.lastModifiedUnixUtc = Math.Max(result.lastModifiedUnixUtc, other.lastModifiedUnixUtc);

            if (string.IsNullOrWhiteSpace(result.deviceId))
                result.deviceId = other.deviceId;

            return result;
        }

        private static bool IsCloudNewer(PlayerProgressData local, PlayerProgressData cloud)
        {
            if (cloud.progressRevision != local.progressRevision)
                return cloud.progressRevision > local.progressRevision;

            if (cloud.lastModifiedUnixUtc != local.lastModifiedUnixUtc)
                return cloud.lastModifiedUnixUtc > local.lastModifiedUnixUtc;

            return true;
        }

        private static string[] UnionStrings(string[] a, string[] b)
        {
            if (a == null || a.Length == 0) return b;
            if (b == null || b.Length == 0) return a;

            var set = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < a.Length; i++)
                if (!string.IsNullOrWhiteSpace(a[i])) set.Add(a[i]);

            for (int i = 0; i < b.Length; i++)
                if (!string.IsNullOrWhiteSpace(b[i])) set.Add(b[i]);

            var arr = new string[set.Count];
            int k = 0;
            foreach (var s in set) arr[k++] = s;
            return arr;
        }
    }
}
