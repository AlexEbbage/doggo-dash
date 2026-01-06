using UnityEngine;
using Game.Application.Services;
using Game.Application.Ports;
using Game.Infrastructure.Persistence;
using Game.Domain.ValueObjects;
using Game.Presentation.Runtime.Input;
using Game.Presentation.Runtime.Run;

namespace Game.Presentation.Runtime.Missions
{
    public sealed class MissionSystemBehaviour : MonoBehaviour, IScoreMultiplierProvider
    {
        [Header("Data")]
        public MissionCatalogSO catalog = default!;

        [Header("Refs")]
        public SwipeInputBehaviour swipeInput = default!;
        public MonoBehaviour runnerDistanceProviderBehaviour = default!; // IRunnerDistanceProvider
        public RunStateControllerBehaviour runState = default!;

        [Header("Config")]
        public int maxMultiplierLevel = 30;

        [Header("Debug")]
        public bool log = true;

        private IRunnerDistanceProvider _distance = default!;
        private IProgressSaveGateway _save = default!;
        private PlayerProgressData _data = default!;
        private MissionManager _manager = default!;
        private System.Random _rng;

        private float _lastWholeMeters;

        public MissionManager Manager => _manager;

        public int ScoreMultiplier => 1 + Mathf.Clamp(_data != null ? _data.missionMultiplierLevel : 0, 0, maxMultiplierLevel);

        private void Awake()
        {
            if (catalog == null)
            {
                Debug.LogError("[MissionSystem] catalog not assigned.");
                enabled = false;
                return;
            }

            _distance = runnerDistanceProviderBehaviour as IRunnerDistanceProvider;
            if (_distance == null)
            {
                Debug.LogError("[MissionSystem] runnerDistanceProviderBehaviour must implement IRunnerDistanceProvider.");
                enabled = false;
                return;
            }

            _save = new PlayerPrefsProgressSaveGateway();
            _data = _save.Load();
            if (_data.ownedItemIds == null) _data.ownedItemIds = new string[0]; // defensive

            _manager = new MissionManager();
            _rng = new System.Random();

            if (_data.activeMissions != null && _data.activeMissions.Length > 0)
                _manager.LoadFromSaved(catalog, _data.activeMissions);
            else
                RerollAndSave();

            _manager.OnMissionCompleted += OnMissionCompleted;

            if (swipeInput != null)
                swipeInput.OnCommand += OnCommand;
        }

        private void OnDestroy()
        {
            if (swipeInput != null)
                swipeInput.OnCommand -= OnCommand;

            if (_manager != null)
                _manager.OnMissionCompleted -= OnMissionCompleted;
        }

        private void Update()
        {
            if (runState != null && runState.IsFailed) return;

            float whole = Mathf.Floor(_distance.DistanceTravelledMeters);
            int delta = Mathf.FloorToInt(whole - _lastWholeMeters);
            if (delta > 0)
            {
                _lastWholeMeters = whole;
                _manager.AddProgress(MissionType.RunDistanceMeters, delta);
            }
        }

        private void OnCommand(RunnerCommandType cmd)
        {
            if (cmd == RunnerCommandType.Jump)
                _manager.AddProgress(MissionType.Jump, 1);
            else if (cmd == RunnerCommandType.Slide)
                _manager.AddProgress(MissionType.Slide, 1);
        }

        public void ReportTreatCollected(int amount) => _manager.AddProgress(MissionType.CollectTreats, Mathf.Max(0, amount));
        public void ReportGemCollected(int amount) => _manager.AddProgress(MissionType.CollectGems, Mathf.Max(0, amount));

        private void OnMissionCompleted(MissionManager.ActiveMission m)
        {
            if (log) Debug.Log($"[Mission] Completed {m.Definition.displayName} ({m.Progress.Type})");

            _data.missionMultiplierLevel = Mathf.Clamp(_data.missionMultiplierLevel + 1, 0, maxMultiplierLevel);
            PersistMissions();

            if (_manager.AreAllCompleted())
            {
                RerollAndSave();
                if (log) Debug.Log("[Mission] All complete → rerolled");
            }
        }

        private void RerollAndSave()
        {
            _manager.InitialiseNew(catalog, _rng);
            PersistMissions();
        }

        private void PersistMissions()
        {
            _data.activeMissions = _manager.ToSaved();
            _save.Save(_data);
        }
    }
}
