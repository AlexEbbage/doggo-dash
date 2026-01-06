using Game.Domain.ValueObjects;

namespace Game.Application.Ports
{
    public interface IRunnerEventSink
    {
        void OnLaneChangeStarted(Lane from, Lane to);
        void OnJumpStarted();
        void OnSlideStarted();
        void OnSlideEnded();
    }
}
