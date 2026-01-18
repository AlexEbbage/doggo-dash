using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using UnityEngine;

namespace Game.Presentation.Runtime.Hub
{
    public sealed class HubProgressBootstrapperBehaviour : MonoBehaviour
    {
        private void Awake()
        {
            var gateway = new PlayerPrefsProgressSaveGateway();
            bool isValid = gateway.TryLoadValidated(out PlayerProgressData data, out bool needsSave);

            if (!isValid)
            {
                gateway.Clear();
            }

            if (needsSave)
            {
                gateway.Save(data);
            }
        }
    }
}
