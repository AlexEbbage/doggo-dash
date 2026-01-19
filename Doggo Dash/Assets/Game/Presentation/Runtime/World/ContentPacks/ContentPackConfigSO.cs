using UnityEngine;
using Game.Presentation.Runtime.World.Obstacles;
using Game.Presentation.Runtime.World.Pickups;
using Game.Presentation.Runtime.World.Track;

namespace Game.Presentation.Runtime.World.ContentPacks
{
    [CreateAssetMenu(menuName = "Game/Configs/Content Pack Config", fileName = "ContentPackConfig")]
    public sealed class ContentPackConfigSO : ScriptableObject
    {
        [Header("Active Packs")]
        public ContentPackSO[] activePacks = default!;

        [Header("Targets")]
        public TrackBiomeConfigSO[] biomeTargets = default!;
        public ObstaclePatternSO[] obstaclePatternTargets = default!;
        public PickupPatternSO[] pickupPatternTargets = default!;
    }
}
