namespace Game.Application.Ports
{
    /// <summary>
    /// Anything that contributes to forward speed scaling implements this.
    /// Multipliers stack multiplicatively.
    /// </summary>
    public interface ISpeedModifierSource
    {
        float SpeedMultiplier { get; }
    }
}
