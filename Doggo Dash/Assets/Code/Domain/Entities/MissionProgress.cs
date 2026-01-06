using Game.Domain.ValueObjects;

namespace Game.Domain.Entities
{
    [System.Serializable]
    public sealed class MissionProgress
    {
        public MissionType Type;
        public int Target;
        public int Current;
        public bool Completed;

        public MissionProgress(MissionType type, int target, int current = 0, bool completed = false)
        {
            Type = type;
            Target = target;
            Current = current;
            Completed = completed || (Target > 0 && Current >= Target);
            if (Completed) Current = Target;
        }

        public void Add(int amount)
        {
            if (Completed) return;

            Current += amount;
            if (Current >= Target)
            {
                Current = Target;
                Completed = true;
            }
        }

        public float Normalized => Target <= 0 ? 1f : (float)Current / Target;
    }
}
