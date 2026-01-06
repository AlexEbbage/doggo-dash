namespace Game.Presentation.Runtime.Playtest
{
    public interface ICloudSaveStatusSource
    {
        string ProviderName { get; }
        string SlotKey { get; }
        string LastStatus { get; }
        long LastSyncUnixUtc { get; }
    }
}
