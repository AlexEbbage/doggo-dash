using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Input
{
    public sealed class KeyboardInputBehaviour : MonoBehaviour, IRunnerCommandSource
    {
        public event System.Action<RunnerCommandType> OnCommand;

        public KeyCode laneLeft = KeyCode.A;
        public KeyCode laneRight = KeyCode.D;
        public KeyCode jump = KeyCode.W;
        public KeyCode slide = KeyCode.S;

        public KeyCode laneLeftAlt = KeyCode.LeftArrow;
        public KeyCode laneRightAlt = KeyCode.RightArrow;
        public KeyCode jumpAlt = KeyCode.UpArrow;
        public KeyCode slideAlt = KeyCode.DownArrow;

        public bool onlyInEditor = true;

        private void Update()
        {
            if (onlyInEditor && !UnityEngine.Application.isEditor)
                return;

            if (UnityEngine.Input.GetKeyDown(laneLeft) ||
                UnityEngine.Input.GetKeyDown(laneLeftAlt))
                OnCommand?.Invoke(RunnerCommandType.MoveLeft);

            if (UnityEngine.Input.GetKeyDown(laneRight) ||
                UnityEngine.Input.GetKeyDown(laneRightAlt))
                OnCommand?.Invoke(RunnerCommandType.MoveRight);

            if (UnityEngine.Input.GetKeyDown(jump) ||
                UnityEngine.Input.GetKeyDown(jumpAlt))
                OnCommand?.Invoke(RunnerCommandType.Jump);

            if (UnityEngine.Input.GetKeyDown(slide) ||
                UnityEngine.Input.GetKeyDown(slideAlt))
                OnCommand?.Invoke(RunnerCommandType.Slide);
        }
    }
}
