namespace Game.Domain.ValueObjects
{
    public enum PickupType
    {
        Treat,    // grants kibble; restores energy
        BadFood,  // slow + energy penalty
        Gem,      // premium currency
        Zoomies   // powerup: speed boost
    }
}
