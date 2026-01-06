using Game.Domain.ValueObjects;

namespace Game.Domain.Entities
{
    /// <summary>
    /// Pure state for the runner simulation (no Unity types).
    /// </summary>
    public sealed class RunnerState
    {
        public Lane CurrentLane { get; internal set; } = Lane.Middle;

        // Lane transition
        public bool IsChangingLane { get; internal set; }
        public Lane FromLane { get; internal set; } = Lane.Middle;
        public Lane ToLane { get; internal set; } = Lane.Middle;
        public float LaneChangeT { get; internal set; } // 0..1

        // Jump
        public bool IsJumping { get; internal set; }
        public float JumpElapsed { get; internal set; }

        // Slide
        public bool IsSliding { get; internal set; }
        public float SlideElapsed { get; internal set; }
    }
}
