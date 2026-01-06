namespace Game.Application.DTOs
{
    /// <summary>
    /// A Unity-agnostic “frame output” that Presentation can apply to transforms/colliders.
    /// </summary>
    public readonly struct RunnerFrame
    {
        public RunnerFrame(float lateralOffset, float verticalOffset, float forwardSpeed, bool isSliding)
        {
            LateralOffset = lateralOffset;
            VerticalOffset = verticalOffset;
            ForwardSpeed = forwardSpeed;
            IsSliding = isSliding;
        }

        public float LateralOffset { get; }
        public float VerticalOffset { get; }
        public float ForwardSpeed { get; }
        public bool IsSliding { get; }
    }
}
