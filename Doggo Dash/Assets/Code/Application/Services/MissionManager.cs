using System;
using System.Collections.Generic;
using Game.Application.DTOs;
using Game.Domain.ValueObjects;
using Game.Application.Ports;

namespace Game.Application.Services
{
    public sealed class MissionManager
    {
        public const int ActiveCount = 3;

        public sealed class ActiveMission
        {
            public MissionDefinitionData Definition;
            public Game.Domain.Entities.MissionProgress Progress;

            public ActiveMission(MissionDefinitionData def, Game.Domain.Entities.MissionProgress prog)
            {
                Definition = def;
                Progress = prog;
            }
        }

        private readonly List<ActiveMission> _active = new(ActiveCount);
        public IReadOnlyList<ActiveMission> Active => _active;

        public event Action<ActiveMission>? OnMissionCompleted;

        public void InitialiseNew(MissionDefinitionData[] pool, Random rng)
        {
            _active.Clear();
            if (pool == null || pool.Length == 0) return;

            var weighted = new List<int>(pool.Length * 2);
            for (int i = 0; i < pool.Length; i++)
            {
                int w = Math.Max(1, pool[i].Weight);
                for (int k = 0; k < w; k++) weighted.Add(i);
            }
            if (weighted.Count == 0) return;

            var chosen = new HashSet<MissionType>();

            while (_active.Count < ActiveCount && chosen.Count < 64)
            {
                int idx = weighted[rng.Next(0, weighted.Count)];
                var def = pool[idx];
                if (chosen.Contains(def.Type)) continue;

                chosen.Add(def.Type);
                _active.Add(new ActiveMission(def, new Game.Domain.Entities.MissionProgress(def.Type, def.Target)));
            }
        }

        public void LoadFromSaved(SavedMissionData[] saved)
        {
            _active.Clear();
            if (saved == null || saved.Length == 0) return;

            for (int i = 0; i < saved.Length; i++)
            {
                var s = saved[i];
                if (s == null) continue;

                var def = new MissionDefinitionData
                {
                    Type = s.type,
                    Target = Math.Max(1, s.target),
                    RewardKibble = Math.Max(0, s.rewardKibble),
                    RewardGems = Math.Max(0, s.rewardGems),
                    Weight = 1,
                    DisplayName = s.displayName ?? s.type.ToString()
                };

                var prog = new Game.Domain.Entities.MissionProgress(s.type, s.target, s.current, s.completed);
                _active.Add(new ActiveMission(def, prog));
            }
        }

        public SavedMissionData[] ToSaved()
        {
            var arr = new SavedMissionData[_active.Count];
            for (int i = 0; i < _active.Count; i++)
            {
                var a = _active[i];
                arr[i] = new SavedMissionData
                {
                    type = a.Progress.Type,
                    target = a.Progress.Target,
                    current = a.Progress.Current,
                    completed = a.Progress.Completed,
                    rewardKibble = a.Definition.RewardKibble,
                    rewardGems = a.Definition.RewardGems,
                    displayName = a.Definition.DisplayName
                };
            }
            return arr;
        }

        public void AddProgress(MissionType type, int amount)
        {
            for (int i = 0; i < _active.Count; i++)
            {
                var a = _active[i];
                if (a.Progress.Completed) continue;
                if (a.Progress.Type != type) continue;

                bool before = a.Progress.Completed;
                a.Progress.Add(amount);

                if (!before && a.Progress.Completed)
                    OnMissionCompleted?.Invoke(a);
            }
        }
    }
}
