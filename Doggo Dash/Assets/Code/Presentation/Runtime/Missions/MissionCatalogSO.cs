using UnityEngine;

namespace Game.Presentation.Runtime.Missions
{
    [CreateAssetMenu(menuName = "Game/Missions/Mission Catalog", fileName = "MissionCatalog")]
    public sealed class MissionCatalogSO : ScriptableObject
    {
        public MissionDefinitionSO[] missions;
    }
}
