using System.Collections.Generic;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.Meta
{
    public static class MetaProgressTextFormatter
    {
        public static string BuildSelectionLabel(string label, string id)
        {
            string resolved = string.IsNullOrWhiteSpace(id) ? "-" : id;
            return $"{label}: {resolved}";
        }

        public static string BuildChallengeSummary(PlayerProgressData data)
        {
            if (data == null || data.challengeProgress == null || data.challengeProgress.Count == 0)
            {
                return BuildChallengeSummary(0, 0, 0);
            }

            int total = data.challengeProgress.Count;
            int completed = 0;
            int claimed = 0;

            foreach (ChallengeProgress entry in data.challengeProgress)
            {
                if (entry == null)
                {
                    continue;
                }

                if (entry.completed)
                {
                    completed++;
                }

                if (entry.rewardClaimed)
                {
                    claimed++;
                }
            }

            return BuildChallengeSummary(completed, total, claimed);
        }

        public static string BuildChallengeSummary(int completed, int total, int claimed)
        {
            return $"Challenges: {completed}/{total} complete ({claimed} claimed)";
        }

        public static string BuildOwnedSummary(IReadOnlyCollection<string> pets, IReadOnlyCollection<string> outfits)
        {
            int petsOwned = pets?.Count ?? 0;
            int outfitsOwned = outfits?.Count ?? 0;
            return $"Owned: {petsOwned} pets, {outfitsOwned} outfits";
        }
    }
}
