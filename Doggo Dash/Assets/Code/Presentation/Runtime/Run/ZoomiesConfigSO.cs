using UnityEngine;

namespace Game.Presentation.Runtime.Run
{
    [CreateAssetMenu(menuName = "Game/Configs/Zoomies Config", fileName = "ZoomiesConfig")]
    public sealed class ZoomiesConfigSO : ScriptableObject
    {
        [Min(1f)] public float speedMultiplier = 1.5f;
        [Min(0.1f)] public float durationSeconds = 3.0f;

        public bool refreshOnPickup = true;
        public bool allowPickupWhileActive = true;
    }
}
