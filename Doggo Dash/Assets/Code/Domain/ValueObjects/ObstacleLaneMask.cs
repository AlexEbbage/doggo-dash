using System;

namespace Game.Domain.ValueObjects
{
    [Flags]
    public enum ObstacleLaneMask
    {
        None = 0,
        Left = 1 << 0,
        Middle = 1 << 1,
        Right = 1 << 2,
        All = Left | Middle | Right
    }
}
