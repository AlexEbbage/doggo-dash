using UnityEngine;

namespace Game.Presentation.Runtime.Events
{
    [CreateAssetMenu(menuName = "Game/Events/Event Definition", fileName = "Event_")]
    public sealed class EventDefinitionSO : ScriptableObject
    {
        public string eventId = "spring_event";
        public string displayName = "Spring Festival";

        public string startUtcIso;
        public string endUtcIso;

        public long startUnixUtc;
        public long endUnixUtc;

        public bool enableEventMissions = true;
        public bool enableEventCurrency = true;

        public string eventCurrencyId = "flowers";
        public int currencyPerPickup = 1;

        public EventShopCatalogSO shopCatalog;
    }
}
