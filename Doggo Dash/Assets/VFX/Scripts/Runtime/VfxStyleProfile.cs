using UnityEngine;

namespace DoggoDash.Vfx
{
    [CreateAssetMenu(menuName = "VFX/VFX Style Profile", fileName = "VfxStyleProfile")]
    public class VfxStyleProfile : ScriptableObject
    {
        public Color PrimaryColor = new(1f, 0.9f, 0.6f, 1f);
        public Color SecondaryColor = new(0.6f, 0.9f, 1f, 1f);
        public Color AccentColor = new(1f, 0.4f, 0.2f, 1f);
        public Color SmokeColor = new(0.8f, 0.8f, 0.8f, 1f);
        public float Intensity = 1f;
        public bool UseGlow = true;
        public string SortingLayerWorld = "Default";
        public string SortingLayerOverlay = "Default";
        public float GlobalScale = 1f;
    }
}
