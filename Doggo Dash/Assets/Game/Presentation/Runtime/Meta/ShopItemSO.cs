using UnityEngine;

namespace Game.Presentation.Runtime.Meta
{
    public enum ShopCurrency
    {
        Kibble,
        Gems
    }

    public enum ShopItemType
    {
        Pet,
        Outfit,
        GemPack
    }

    [CreateAssetMenu(menuName = "Game/Meta/Shop Item", fileName = "ShopItem")]
    public sealed class ShopItemSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemId = "dog_default";
        public ShopItemType type = ShopItemType.Pet;

        [Header("Display")]
        public string displayName = "Default Dog";
        public Sprite icon;

        [Header("Cost")]
        public ShopCurrency currency = ShopCurrency.Kibble;
        [Min(0)] public int price = 0;

        [Header("Gem Pack")]
        [Min(0)] public int gemAmount = 0;

        [Header("IAP")]
        public string iapProductId = string.Empty;
    }
}
