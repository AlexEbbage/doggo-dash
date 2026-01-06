namespace Game.Presentation.Runtime.Pooling
{
    public interface IPoolable
    {
        void OnSpawned();
        void OnDespawned();
    }
}
