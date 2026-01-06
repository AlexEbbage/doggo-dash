using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.World.Obstacles
{
    public sealed class ObstacleView : MonoBehaviour
    {
        [Header("Obstacle")]
        public ObstacleType obstacleType = ObstacleType.FullBlock;
        public ObstacleLaneMask laneMask = ObstacleLaneMask.Middle;
        public bool isFatal = true;
    }
}
