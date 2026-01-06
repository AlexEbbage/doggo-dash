using UnityEngine;
using Game.Application.Ports;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Runner
{
    public sealed class UnityRunnerEventSink : MonoBehaviour, IRunnerEventSink
    {
        [Header("Optional")]
        public Animator? animator;

        private static readonly int HashJump = Animator.StringToHash("Jump");
        private static readonly int HashSlide = Animator.StringToHash("Slide");
        private static readonly int HashLane = Animator.StringToHash("Lane");

        public void OnLaneChangeStarted(Lane from, Lane to)
        {
            if (animator != null)
                animator.SetInteger(HashLane, (int)to);
        }

        public void OnJumpStarted()
        {
            if (animator != null)
                animator.SetTrigger(HashJump);
        }

        public void OnSlideStarted()
        {
            if (animator != null)
                animator.SetBool(HashSlide, true);
        }

        public void OnSlideEnded()
        {
            if (animator != null)
                animator.SetBool(HashSlide, false);
        }
    }
}
