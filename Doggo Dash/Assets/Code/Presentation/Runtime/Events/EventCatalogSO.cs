using System;
using System.Globalization;
using UnityEngine;

namespace Game.Presentation.Runtime.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Event Catalog", fileName = "EventCatalog")]
    public sealed class EventCatalogSO : ScriptableObject
    {
        public EventDefinitionSO[] events;

        public EventDefinitionSO GetActiveEvent(DateTime utcNow)
        {
            if (events == null) return null;

            for (int i = 0; i < events.Length; i++)
            {
                var e = events[i];
                if (e == null) continue;

                if (!TryGetWindowUtc(e, out var startUtc, out var endUtc))
                    continue;

                if (utcNow >= startUtc && utcNow <= endUtc)
                    return e;
            }

            return null;
        }

        private static bool TryGetWindowUtc(EventDefinitionSO e, out DateTime startUtc, out DateTime endUtc)
        {
            startUtc = default;
            endUtc = default;

            if (e.startUnixUtc > 0 && e.endUnixUtc > 0)
            {
                startUtc = DateTimeOffset.FromUnixTimeSeconds(e.startUnixUtc).UtcDateTime;
                endUtc = DateTimeOffset.FromUnixTimeSeconds(e.endUnixUtc).UtcDateTime;
                return true;
            }

            if (string.IsNullOrWhiteSpace(e.startUtcIso) || string.IsNullOrWhiteSpace(e.endUtcIso))
                return false;

            if (!DateTimeOffset.TryParse(e.startUtcIso, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var start))
                return false;

            if (!DateTimeOffset.TryParse(e.endUtcIso, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var end))
                return false;

            startUtc = start.UtcDateTime;
            endUtc = end.UtcDateTime;
            return true;
        }
    }
}
