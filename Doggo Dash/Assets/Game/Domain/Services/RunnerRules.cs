using Game.Domain.ValueObjects;

namespace Game.Domain.Services
{
    public static class RunnerRules
    {
        public static bool CanStartLaneChange(bool isChangingLane, bool isSliding)
        {
            // MVP: allow lane change while jumping, disallow while sliding for clarity.
            return !isChangingLane && !isSliding;
        }

        public static Lane ClampLane(int laneValue)
        {
            if (laneValue < (int)Lane.Left) return Lane.Left;
            if (laneValue > (int)Lane.Right) return Lane.Right;
            return (Lane)laneValue;
        }
    }
}
