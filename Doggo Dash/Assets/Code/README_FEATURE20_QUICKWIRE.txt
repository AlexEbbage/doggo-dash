FEATURE 20 (Missions & Multipliers) – QUICK WIRING

1) Create assets:
   - MissionCatalogSO (Create > Game > Missions > Mission Catalog)
   - 8–12 MissionDefinitionSO assets (Create > Game > Missions > Mission Definition)
     Example mission types: CollectTreats, CollectGems, Jump, Slide, RunDistanceMeters

2) Game Scene objects:
   - Create GameObject 'Missions'
     Add MissionSystemBehaviour:
       catalog = your MissionCatalog
       swipeInput = SwipeInputBehaviour in scene
       runnerDistanceProviderBehaviour = RunnerDistanceProviderBehaviour (IRunnerDistanceProvider)
       runState = RunStateControllerBehaviour

   - Add MissionPickupSinkBehaviour somewhere:
       missionSystem = Missions/MissionSystemBehaviour
     Then assign it into your PickupEventRouterBehaviour where it accepts IPickupCollectedSink.

   - Add MissionRewardsBehaviour:
       missionSystem = MissionSystemBehaviour
       runRewards = RunRewardTrackerBehaviour
     IMPORTANT: RunRewardTrackerBehaviour must expose:
       public void AddKibble(int amount)
       public void AddGems(int amount)

3) Score multiplier:
   - MissionSystemBehaviour implements IScoreMultiplierProvider (ScoreMultiplier property).
   - Your ScoreDistanceControllerBehaviour should optionally read an IScoreMultiplierProvider
     and multiply score gain by it.
