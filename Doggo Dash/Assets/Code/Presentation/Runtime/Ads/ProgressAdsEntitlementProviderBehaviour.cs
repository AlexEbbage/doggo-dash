using UnityEngine;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;

namespace Game.Presentation.Runtime.Ads
{
    public sealed class ProgressAdsEntitlementProviderBehaviour : MonoBehaviour, IAdsEntitlementProvider
    {
        public bool RemoveAdsOwned
        {
            get
            {
                IProgressSaveGateway save = new PlayerPrefsProgressSaveGateway();
                var data = save.Load();
                return data.removeAdsOwned;
            }
        }
    }
}
