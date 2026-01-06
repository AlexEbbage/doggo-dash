using Game.Domain.ValueObjects;

namespace Game.Application.Ports
{
    public interface IRunFailSink
    {
        void OnRunFailed(RunFailReason reason, ObstacleType? obstacleType = null);
    }

    public enum RunFailReason
    {
        ObstacleHit,
        EnergyDepleted
    }
}
