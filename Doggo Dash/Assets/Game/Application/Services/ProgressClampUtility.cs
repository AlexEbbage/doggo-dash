using Game.Application.Ports;

namespace Game.Application.Services
{
    public static class ProgressClampUtility
    {
        public static void ClampProgress(PlayerProgressData data)
        {
            if (data == null) return;

            if (data.xp < 0) data.xp = 0;
            if (data.xpToNext < 1) data.xpToNext = 1;
            if (data.xp > data.xpToNext) data.xp = data.xpToNext;

            if (data.energyMax < 1f) data.energyMax = 1f;
            if (data.energyCurrent < 0f) data.energyCurrent = 0f;
            if (data.energyCurrent > data.energyMax) data.energyCurrent = data.energyMax;
        }
    }
}
