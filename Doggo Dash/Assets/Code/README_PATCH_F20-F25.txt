F20–F25 Fixes Patch Pack
=======================

This patch pack contains the fixes from the Phase 2 review:
- F20 architecture fix: remove Unity ScriptableObjects from Application layer (MissionManager now uses plain MissionDefinitionData).
- F25 pooling fix: remove per-spawn/per-despawn allocations by caching IPoolable components per instance.
- VFX lifetime fix: coroutine-based return-to-pool (no per-frame Update ticking).
- Ads stub fix: WaitForSecondsRealtime so revive works when Time.timeScale=0.
- Deterministic banking fix: ProgressBankBehaviour now banks explicitly via BankNow(multiplier) and never auto-banks in Update.
- Double rewards flow fix: GameOverClaimControllerBehaviour offers Claim vs Watch-to-Double and banks exactly once.
- Remove-ads entitlement gating: per-placement disableWhenRemoveAdsOwned + entitlement provider.
- Mission HUD fix: throttled updates (5Hz).

IMPORTANT:
----------
This pack includes:
- New files (safe to add).
- Full replacements for a few files (ObjectPool.cs, ReturnToPoolAfterSeconds.cs, MissionManager.cs, ProgressBankBehaviour.cs).
- Small edits needed in existing files:
    - IProgressSaveGateway.cs: add SavedMissionData.displayName if not present.
    - MissionSystemBehaviour.cs: map MissionCatalogSO -> MissionDefinitionData[] when initialising new missions.
    - RewardedAdsServiceBehaviour.cs: add optional entitlementProviderBehaviour gating.
    - AdsConfigSO.cs: add disabledWhenRemoveAdsOwned flag to PlacementRule.
    - StubRewardedAdProviderBehaviour.cs: use WaitForSecondsRealtime.
    - MissionHudPresenterBehaviour.cs: throttle refresh.

If you want me to generate exact diff patches against your current files, paste those files or upload your project zip.
