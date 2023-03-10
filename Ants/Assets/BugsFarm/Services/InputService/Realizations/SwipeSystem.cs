using System;
using UnityEngine;
using Zenject;

namespace BugsFarm.Services.InputService
{
    public class SwipeSystem : IInitializable, IDisposable, ISwipeSystem
    {
        public event Action<Vector2> OnSwipe;
        private readonly IInputSystem _inputSystem;
        private readonly float _diagonalLength;

        private const float _minSwipeAngle = 0.85f;
        private const float _minSwipeDistance = 0.05f;

        private Vector2 _bgnSwipe = Vector2.zero;

        public SwipeSystem(IInputSystem inputSystem)
        {
            _inputSystem = inputSystem;
            _diagonalLength = new Vector2(Screen.width, Screen.height).magnitude;
        }

        public void Initialize()
        {
            _inputSystem.OnInput += OnInput;
        }

        public void Dispose()
        {
            _inputSystem.OnInput -= OnInput;
        }

        private void OnInput()
        {
            switch (_inputSystem.Phase)
            {
                case TouchPhase.Began:
                    _bgnSwipe = _inputSystem.Position;
                    break;
                case TouchPhase.Ended:
                    Vector2 currSwipe = Input.mousePosition;
                    var currentDir = _bgnSwipe - currSwipe;

                    // минимальный свайп
                    if (currentDir.magnitude / _diagonalLength < _minSwipeDistance)
                        return;

                    if (!TryConstraintDir(currentDir.normalized, out var dir))
                        return;

                    OnSwipe?.Invoke(dir);
                    _bgnSwipe = Vector2.zero;
                    break;
            }
        }

        private bool TryConstraintDir(Vector2 swipeDir, out Vector2 dir)
        {
            if (Mathf.Abs(swipeDir.x) > _minSwipeAngle)
            {
                dir = swipeDir.x > 0 ? Vector2.right : Vector2.left;
                return true;
            }

            if (Mathf.Abs(swipeDir.y) > _minSwipeAngle)
            {
                dir = swipeDir.y > 0 ? Vector2.up : Vector2.down;
                return true;
            }

            dir = Vector2.zero;
            return false;
        }
    }
}