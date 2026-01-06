using UnityEngine;
using Game.Domain.ValueObjects;

namespace Game.Presentation.Runtime.Input
{
    public sealed class SwipeInputBehaviour : MonoBehaviour
    {
        [Header("Swipe")]
        [Min(10f)] public float minSwipeDistancePixels = 60f;
        public bool ignoreTaps = true;

        public System.Action<RunnerCommandType>? OnCommand;

        private bool _tracking;
        private Vector2 _startPos;

        private void Update()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch t = UnityEngine.Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    _tracking = true;
                    _startPos = t.position;
                }
                else if (_tracking && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
                {
                    _tracking = false;
                    EmitSwipe(_startPos, t.position);
                }
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _tracking = true;
                _startPos = UnityEngine.Input.mousePosition;
            }
            else if (_tracking && UnityEngine.Input.GetMouseButtonUp(0))
            {
                _tracking = false;
                EmitSwipe(_startPos, (Vector2)UnityEngine.Input.mousePosition);
            }
#endif
        }

        private void EmitSwipe(Vector2 start, Vector2 end)
        {
            Vector2 delta = end - start;
            float dist = delta.magnitude;

            if (dist < minSwipeDistancePixels)
            {
                if (!ignoreTaps)
                {
                    // taps reserved
                }
                return;
            }

            bool horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);

            if (horizontal)
                OnCommand?.Invoke(delta.x < 0 ? RunnerCommandType.MoveLeft : RunnerCommandType.MoveRight);
            else
                OnCommand?.Invoke(delta.y < 0 ? RunnerCommandType.Slide : RunnerCommandType.Jump);
        }
    }
}
