using System.Collections.Generic;
using UnityEngine;
using Game.Application.Ports;
using Game.Application.DTOs;

namespace Game.Presentation.Runtime.Analytics
{
    public sealed class AnalyticsServiceBehaviour : MonoBehaviour
    {
        public MonoBehaviour[] sinkBehaviours;
        public bool logSessionStartOnAwake = true;

        private readonly List<IAnalyticsSink> _sinks = new(4);

        private void Awake()
        {
            _sinks.Clear();
            if (sinkBehaviours != null)
            {
                for (int i = 0; i < sinkBehaviours.Length; i++)
                {
                    var s = sinkBehaviours[i] as IAnalyticsSink;
                    if (s != null) _sinks.Add(s);
                }
            }

            if (logSessionStartOnAwake)
            {
                Track(AnalyticsEventNames.AppSessionStart, new Dictionary<string, object>
                {
                    { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name },
                    { "platform", Application.platform.ToString() },
                    { "version", Application.version },
                });
            }
        }

        public void Track(string eventName, IReadOnlyDictionary<string, object> parameters)
        {
            if (string.IsNullOrWhiteSpace(eventName)) return;
            for (int i = 0; i < _sinks.Count; i++) _sinks[i].Track(eventName, parameters);
        }
    }
}
