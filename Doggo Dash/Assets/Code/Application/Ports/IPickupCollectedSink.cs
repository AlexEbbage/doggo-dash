using Game.Domain.ValueObjects;

namespace Game.Application.Ports
{
    /// <summary>
    /// Presentation reports pickups; Application/Meta can consume later.
    /// </summary>
    public interface IPickupCollectedSink
    {
        void OnPickupCollected(PickupType type, int amount);
    }
}
