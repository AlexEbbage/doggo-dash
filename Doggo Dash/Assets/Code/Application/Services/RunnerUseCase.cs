using Game.Application.DTOs;
using Game.Application.Ports;
using Game.Domain.Entities;
using Game.Domain.Services;
using Game.Domain.ValueObjects;

namespace Game.Application.Services
{
    /// <summary>
    /// Orchestrates runner commands and produces per-frame motion outputs.
    /// </summary>
    public sealed class RunnerUseCase
    {
        private readonly RunnerConfig _config;
        private readonly RunnerState _state;
        private readonly IRunnerEventSink _events;

        private float _forwardSpeed;

        public RunnerUseCase(RunnerConfig config, RunnerState state, IRunnerEventSink events)
        {
            _config = config;
            _state = state;
            _events = events;

            _forwardSpeed = config.BaseForwardSpeed;
        }

        public RunnerState State => _state;

        public void SetForwardSpeed(float forwardSpeed)
        {
            _forwardSpeed = forwardSpeed < 0f ? 0f : forwardSpeed;
        }

        public void HandleCommand(RunnerCommandType cmd)
        {
            switch (cmd)
            {
                case RunnerCommandType.MoveLeft:
                    TryStartLaneChange(-1);
                    break;
                case RunnerCommandType.MoveRight:
                    TryStartLaneChange(+1);
                    break;
                case RunnerCommandType.Jump:
                    TryStartJump();
                    break;
                case RunnerCommandType.Slide:
                    TryStartSlide();
                    break;
            }
        }

        public RunnerFrame Tick(float deltaTime)
        {
            // Lane change simulation
            if (_state.IsChangingLane)
            {
                float dur = _config.LaneChangeDuration <= 0.0001f ? 0.0001f : _config.LaneChangeDuration;
                _state.LaneChangeT += deltaTime / dur;

                if (_state.LaneChangeT >= 1f)
                {
                    _state.LaneChangeT = 1f;
                    _state.IsChangingLane = false;
                    _state.CurrentLane = _state.ToLane;
                }
            }

            // Jump simulation (simple parabola)
            float y = 0f;
            if (_state.IsJumping)
            {
                _state.JumpElapsed += deltaTime;
                float dur = _config.JumpDuration <= 0.0001f ? 0.0001f : _config.JumpDuration;
                float t = _state.JumpElapsed / dur;

                if (t >= 1f)
                {
                    _state.IsJumping = false;
                    _state.JumpElapsed = 0f;
                    y = 0f;
                }
                else
                {
                    y = 4f * _config.JumpHeight * t * (1f - t);
                }
            }

            // Slide simulation
            if (_state.IsSliding)
            {
                _state.SlideElapsed += deltaTime;
                float dur = _config.SlideDuration <= 0.0001f ? 0.0001f : _config.SlideDuration;

                if (_state.SlideElapsed >= dur)
                {
                    _state.IsSliding = false;
                    _state.SlideElapsed = 0f;
                    _events.OnSlideEnded();
                }
            }

            float lateral = ComputeLateralOffset();
            return new RunnerFrame(lateral, y, _forwardSpeed, _state.IsSliding);
        }

        private void TryStartLaneChange(int direction)
        {
            if (!RunnerRules.CanStartLaneChange(_state.IsChangingLane, _state.IsSliding))
                return;

            int current = (int)_state.CurrentLane;
            int target = current + direction;
            Lane toLane = RunnerRules.ClampLane(target);

            if (toLane == _state.CurrentLane) return;

            _state.IsChangingLane = true;
            _state.FromLane = _state.CurrentLane;
            _state.ToLane = toLane;
            _state.LaneChangeT = 0f;

            _events.OnLaneChangeStarted(_state.FromLane, _state.ToLane);
        }

        private void TryStartJump()
        {
            if (_state.IsSliding) return;
            if (_state.IsJumping) return;

            _state.IsJumping = true;
            _state.JumpElapsed = 0f;
            _events.OnJumpStarted();
        }

        private void TryStartSlide()
        {
            if (_state.IsJumping) return;
            if (_state.IsSliding) return;

            _state.IsSliding = true;
            _state.SlideElapsed = 0f;
            _events.OnSlideStarted();
        }

        private float ComputeLateralOffset()
        {
            float laneWidth = _config.LaneWidth;

            if (!_state.IsChangingLane)
                return (int)_state.CurrentLane * laneWidth;

            float from = (int)_state.FromLane * laneWidth;
            float to = (int)_state.ToLane * laneWidth;

            float t = _state.LaneChangeT;
            t = t * t * (3f - 2f * t);

            return from + (to - from) * t;
        }
    }
}
