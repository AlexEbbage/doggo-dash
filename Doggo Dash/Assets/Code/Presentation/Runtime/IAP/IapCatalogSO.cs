using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.IAP
{
    [CreateAssetMenu(menuName = "Game/IAP/IAP Catalog", fileName = "IapCatalog")]
    public sealed class IapCatalogSO : ScriptableObject
    {
        [System.Serializable]
        public sealed class Entry
        {
            public IapProductId id;
            public string storeId = "com.yourgame.gems.small";

            [Header("Rewards")]
            [Min(0)] public int grantGems = 0;
            public bool grantsRemoveAds = false;

            [Header("UI")]
            public string displayName = "Gems Small";
            public string priceString = "$0.99";
        }

        public Entry[] entries;

        public bool TryGet(IapProductId id, out Entry entry)
        {
            if (entries != null)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    var e = entries[i];
                    if (e != null && e.id == id)
                    {
                        entry = e;
                        return true;
                    }
                }
            }

            entry = null!;
            return false;
        }
    }
}
