using UnityEngine;
using Game.Presentation.Runtime.World.Decor;
using Game.Presentation.Runtime.World.Obstacles;
using Game.Presentation.Runtime.World.Pickups;
using Game.Presentation.Runtime.World.Track;

namespace Game.Presentation.Runtime.World.ContentPacks
{
    [CreateAssetMenu(menuName = "Game/Configs/Content Pack", fileName = "ContentPack_")]
    public sealed class ContentPackSO : ScriptableObject
    {
        [Header("Identity")]
        public string packId = "Farm";

        [Header("Track Segments")]
        public TrackSegmentBehaviour[] segmentPrefabs = default!;

        [Header("Decor Configs")]
        public FarmDecorationConfigSO[] decorationConfigs = default!;

        [Header("Obstacle Patterns")]
        public ObstaclePatternSO[] obstaclePatterns = default!;

        [Header("Pickup Patterns")]
        public PickupPatternSO[] pickupPatterns = default!;
    }
}
