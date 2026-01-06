PHASE 2 + PHASE 3 CODE EXPORT (from ChatGPT session)
====================================================

This zip contains the C# scripts produced in-chat for Phase 2 and Phase 3.
It is importable into your Unity project under Assets/.

IMPORTANT: This is NOT a full project export. It does NOT include:
- Scene files, prefabs, materials, textures, audio, shaders
- Any scripts not explicitly written in chat

MANUAL PATCHES REQUIRED (because these touch your existing files):
1) PlayerProgressData (IProgressSaveGateway.cs):
   Add fields used by Phase2/3:
   - bool removeAdsOwned;
   - int eventCurrency; string lastEventId;
   - long progressRevision; long lastModifiedUnixUtc; string deviceId;
   - SavedMissionData: add string displayName;

2) AdsConfigSO (AdsConfigSO.cs):
   PlacementRule: add bool disabledWhenRemoveAdsOwned = true;

3) TrackSpawnerBehaviour:
   Ensure method exists:
   - public void SetActiveTrackSet(TrackSetSO newSet) { activeTrackSet = newSet; /* rebuild cache if any */ }

4) RewardedAdsServiceBehaviour:
   Add entitlement gating (if not already):
   - entitlementProviderBehaviour (IAdsEntitlementProvider) and check rule.disabledWhenRemoveAdsOwned

5) MissionSystemBehaviour:
   Ensure it maps ScriptableObjects to MissionDefinitionData and uses MissionManager (Unity-free).
