using System.Text;
using System.Security.Cryptography;
using UnityEngine;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.Playtest
{
    public static class DebugReportBuilder
    {
        public static string Build(PlayerProgressData data, string extraBlock = null)
        {
            var sb = new StringBuilder(700);

            sb.AppendLine("=== DOG RUNNER DEBUG REPORT ===");
            sb.AppendLine($"Version: {Application.version}");
            sb.AppendLine($"Unity: {Application.unityVersion}");
            sb.AppendLine($"Platform: {Application.platform}");
            sb.AppendLine($"Device: {SystemInfo.deviceModel}");
            sb.AppendLine($"OS: {SystemInfo.operatingSystem}");
            sb.AppendLine($"RAM(MB): {SystemInfo.systemMemorySize}");
            sb.AppendLine($"Cores: {SystemInfo.processorCount}");
            sb.AppendLine($"GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsMemorySize}MB)");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(extraBlock))
            {
                sb.AppendLine("--- Extra ---");
                sb.AppendLine(extraBlock);
                sb.AppendLine();
            }

            if (data != null)
            {
                sb.AppendLine("--- Progress ---");
                sb.AppendLine($"Kibble: {data.totalKibble}");
                sb.AppendLine($"Gems: {data.totalGems}");
                sb.AppendLine($"BestScore: {data.bestScore}");
                sb.AppendLine($"BestDistance: {data.bestDistanceMeters:F1}");
                sb.AppendLine($"SelectedPet: {data.selectedPetId}");
                sb.AppendLine($"SelectedOutfit: {data.selectedOutfitId}");
                sb.AppendLine($"SelectedBiome: {data.selectedBiomeId}");
                sb.AppendLine($"RemoveAdsOwned: {data.removeAdsOwned}");
                sb.AppendLine($"EventCurrency: {data.eventCurrency}");
                sb.AppendLine($"LastEventId: {data.lastEventId}");
                sb.AppendLine($"ProgressRev: {data.progressRevision}");
                sb.AppendLine($"LastModifiedUtc: {data.lastModifiedUnixUtc}");

                string raw = string.IsNullOrWhiteSpace(data.deviceId) ? SystemInfo.deviceUniqueIdentifier : data.deviceId;
                sb.AppendLine($"DeviceIdHash: {HashShort(raw)}");
            }
            else
            {
                sb.AppendLine("Progress: (null)");
            }

            sb.AppendLine();
            sb.AppendLine("Steps to reproduce:");
            sb.AppendLine("1) ...");
            sb.AppendLine("2) ...");
            sb.AppendLine("Expected:");
            sb.AppendLine("Actual:");

            return sb.ToString();
        }

        private static string HashShort(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "unknown";
            try
            {
                using var sha = SHA256.Create();
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
                byte[] hash = sha.ComputeHash(bytes);

                var sb = new StringBuilder(16);
                for (int i = 0; i < 4; i++) sb.Append(hash[i].ToString("x2"));
                return sb.ToString();
            }
            catch
            {
                return "unknown";
            }
        }
    }
}
