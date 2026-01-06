using System.Collections.Generic;
using UnityEngine;
using Game.Application.Ports;

namespace Game.Presentation.Runtime.Analytics
{
    public sealed class DebugAnalyticsSinkBehaviour : MonoBehaviour, IAnalyticsSink
    {
        public bool enabledLogging = true;

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (!enabledLogging) return;
            Debug.Log($"[ANALYTICS] {eventName} ({(parameters != null ? parameters.Count : 0)} params)");
        }
    }
}
