namespace Game.Application.Ports
{
    public interface IProgressSaveGateway
    {
        PlayerProgressData Load();
        void Save(PlayerProgressData data);
    }

    [System.Serializable]
    public sealed class PlayerProgressData
    {
        public int totalKibble;
        public int totalGems;
    }
}
