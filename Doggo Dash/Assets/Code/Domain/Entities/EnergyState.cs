namespace Game.Domain.Entities
{
    /// <summary>
    /// Unity-free energy model. Application controls mutation.
    /// </summary>
    public sealed class EnergyState
    {
        public float Max { get; internal set; }
        public float Current { get; internal set; }

        public EnergyState(float max)
        {
            Max = max > 0f ? max : 1f;
            Current = Max;
        }
    }
}
