using UnityEngine;

namespace DoggoDash.Vfx
{
    public enum VfxColorRole
    {
        Primary,
        Secondary,
        Accent,
        Smoke
    }

    public enum VfxSortingRole
    {
        World,
        Overlay
    }

    [DisallowMultipleComponent]
    public class VfxRendererSettings : MonoBehaviour
    {
        public VfxColorRole ColorRole = VfxColorRole.Primary;
        public VfxSortingRole SortingRole = VfxSortingRole.World;
        public bool AffectStartColor = true;
        public bool AffectMaterialColor = true;
        public bool UseGlowMaterialSwap;
        public Material AdditiveMaterial;
        public Material AlphaMaterial;
        public int SortingOrder = 0;
    }
}
