using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Input
{
    public sealed class RunnerInputMuxBehaviour : MonoBehaviour, IRunnerCommandSource
    {
        public event System.Action<RunnerCommandType> OnCommand;

        [Header("Sources (optional)")]
        public MonoBehaviour swipeBehaviour;
        public MonoBehaviour keyboardBehaviour;

        private IRunnerCommandSource _swipe;
        private IRunnerCommandSource _keyboard;

        private void Awake()
        {
            _swipe = swipeBehaviour as IRunnerCommandSource;
            _keyboard = keyboardBehaviour as IRunnerCommandSource;

            if (swipeBehaviour != null && _swipe == null)
                Debug.LogError("[RunnerInputMux] swipeBehaviour must implement IRunnerCommandSource.");

            if (keyboardBehaviour != null && _keyboard == null)
                Debug.LogError("[RunnerInputMux] keyboardBehaviour must implement IRunnerCommandSource.");
        }

        private void OnEnable()
        {
            if (_swipe != null) _swipe.OnCommand += Forward;
            if (_keyboard != null) _keyboard.OnCommand += Forward;
        }

        private void OnDisable()
        {
            if (_swipe != null) _swipe.OnCommand -= Forward;
            if (_keyboard != null) _keyboard.OnCommand -= Forward;
        }

        private void Forward(RunnerCommandType cmd) => OnCommand?.Invoke(cmd);
    }
}
