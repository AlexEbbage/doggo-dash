namespace Game.Domain.Entities
{
    /// <summary>
    /// Unity-free run-scoped distance + score state.
    /// </summary>
    public sealed class ScoreDistanceState
    {
        public float DistanceMeters { get; internal set; }
        public int Score { get; internal set; }

        public void Reset()
        {
            DistanceMeters = 0f;
            Score = 0;
        }
    }
}
