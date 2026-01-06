using UnityEngine;

namespace Game.Presentation.Runtime.World.Decor
{
    [CreateAssetMenu(menuName = "Game/Configs/Farm Decoration Config", fileName = "FarmDecorationConfig")]
    public sealed class FarmDecorationConfigSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class WeightedPrefab
        {
            public GameObject prefab = default!;
            [Min(0.01f)] public float weight = 1f;
        }

        public WeightedPrefab[] edgeLeftPrefabs = default!;
        public WeightedPrefab[] edgeRightPrefabs = default!;
        public WeightedPrefab[] centerPrefabs = default!;
        public WeightedPrefab[] raisedLeftPrefabs = default!;
        public WeightedPrefab[] raisedRightPrefabs = default!;
        public WeightedPrefab[] backgroundNearPrefabs = default!;
        public WeightedPrefab[] backgroundFarPrefabs = default!;

        [Range(0f, 1f)]
        public float globalSpawnMultiplier = 1f;

        [Min(0f)]
        public float positionJitterXZ = 0.3f;

        [Min(0f)]
        public float minSeparation = 0.6f;
    }
}
