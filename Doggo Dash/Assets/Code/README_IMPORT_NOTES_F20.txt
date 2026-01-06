Additional Bits Package (Features 13–16 + Hardening + Camera)
================================================================

This zip contains ONLY new/replacement scripts added in later steps.

IMPORTANT: A few existing scripts in your project need small edits to integrate:

1) RunnerControllerBehaviour.cs
   - Must expose:
       public float CurrentForwardSpeed { get; private set; }
       public bool IsJumping => ...
       public bool IsSliding => ...
   - CurrentForwardSpeed should be set each Update after computing forward speed.

2) ZoomiesControllerBehaviour.cs
   - Add property: public bool IsActive => _useCase.IsActive;
   - Add optional feedback reference and call on start/end:
       public RunFeedbackControllerBehaviour feedback;
       feedback?.PlayZoomiesStart(); / PlayZoomiesEnd();

3) SpeedCompositorBehaviour.cs
   - Add multiplierSources entries:
       RunDifficultyControllerBehaviour
       EnergySpeedControllerBehaviour
       ZoomiesControllerBehaviour
       StumbleControllerBehaviour

4) TrackSpawnerBehaviour.cs
   - If you use distance-based spawning variant, ensure it depends on IRunnerDistanceProvider.
   - If you reverted to straight-only Z spawning, you can still keep RunDifficulty using RunnerDistanceProviderBehaviour.

This package DOES include:
- RunStateControllerBehaviour.cs (scene reload restart)
- GameOverPanelBehaviour.cs (calls RestartScene)
