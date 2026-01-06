namespace Game.Domain.ValueObjects
{
    public enum ObstacleType
    {
        Low,        // jump over
        High,       // slide under (overhead)
        FullBlock   // must lane change
    }
}
