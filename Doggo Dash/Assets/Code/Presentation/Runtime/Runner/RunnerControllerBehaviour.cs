using UnityEngine;
using Game.Application.Services;
using Game.Domain.Entities;
using Game.Presentation.Runtime.Input;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Runner
{
    /// <summary>
    /// Straight-track runner controller (Subway-style).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class RunnerControllerBehaviour : MonoBehaviour
    {
        [Header("Config")]
        public RunnerConfigSO config = null!;

        [Header("Refs")]
        public SwipeInputBehaviour swipeInput = null!;
        public UnityRunnerEventSink eventSink = null!;

        [Header("Motion")]
        public Transform visualRoot = null!;
        public float groundY = 0f;

        private CharacterController _cc = null!;
        private RunnerUseCase _useCase = null!;
        private float _defaultControllerHeight;
        private Vector3 _defaultControllerCenter;

        private float _targetHeightMultiplier = 1f;
        private float _heightVel;

        private float? _externalForwardSpeed;

        public float BaseForwardSpeed => config != null ? config.baseForwardSpeed : 0f;
        public float CurrentForwardSpeed { get; private set; }

        public void SetExternalForwardSpeed(float speed) => _externalForwardSpeed = Mathf.Max(0f, speed);
        public void ClearExternalForwardSpeed() => _externalForwardSpeed = null;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();

            _defaultControllerHeight = _cc.height;
            _defaultControllerCenter = _cc.center;

            RunnerConfig appConfig = new RunnerConfig
            {
                LaneWidth = config.laneWidth,
                LaneChangeDuration = config.laneChangeDuration,
                JumpHeight = config.jumpHeight,
                JumpDuration = config.jumpDuration,
                SlideDuration = config.slideDuration,
                BaseForwardSpeed = config.baseForwardSpeed
            };

            var state = new RunnerState();
            _useCase = new RunnerUseCase(appConfig, state, eventSink);

            swipeInput.OnCommand += OnCommand;
        }

        private void OnDestroy()
        {
            if (swipeInput != null)
                swipeInput.OnCommand -= OnCommand;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            float forwardSpeed = _externalForwardSpeed ?? BaseForwardSpeed;
            CurrentForwardSpeed = forwardSpeed;

            _useCase.SetForwardSpeed(forwardSpeed);
            var frame = _useCase.Tick(dt);

            Vector3 forwardMove = Vector3.forward * (frame.ForwardSpeed * dt);

            Vector3 pos = transform.position;
            pos.x = frame.LateralOffset;
            pos.y = groundY + frame.VerticalOffset;

            Vector3 toTarget = (pos - transform.position);
            Vector3 delta = toTarget + forwardMove;

            _cc.Move(delta);

            if (visualRoot != null)
            {
                Vector3 v = visualRoot.localPosition;
                v.y = frame.VerticalOffset;
                visualRoot.localPosition = v;
            }

            _targetHeightMultiplier = frame.IsSliding ? config.slideHeightMultiplier : 1f;
            ApplySlideCollider(dt);
        }

        private void OnCommand(RunnerCommandType cmd) => _useCase.HandleCommand(cmd);

        private void ApplySlideCollider(float dt)
        {
            float lerpTime = Mathf.Max(0.0001f, config.slideColliderLerpTime);
            float currentMult = _cc.height / _defaultControllerHeight;
            float newMult = Mathf.SmoothDamp(currentMult, _targetHeightMultiplier, ref _heightVel, lerpTime);

            _cc.height = _defaultControllerHeight * newMult;

            Vector3 center = _defaultControllerCenter;
            float heightDelta = _defaultControllerHeight - _cc.height;
            center.y -= heightDelta * 0.5f;
            _cc.center = center;
        }
    }
}
