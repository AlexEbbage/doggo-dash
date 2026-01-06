using UnityEngine;

namespace Game.Presentation.Runtime.Meta
{
    [CreateAssetMenu(menuName = "Game/Meta/Shop Catalog", fileName = "ShopCatalog")]
    public sealed class ShopCatalogSO : ScriptableObject
    {
        public ShopItemSO[] items;
    }
}
