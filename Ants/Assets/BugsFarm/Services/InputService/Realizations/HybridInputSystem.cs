using System;
using BugsFarm.FarmCameraSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.Services.InputService
{
    public class HybridInputSystem :  ITickable, IInputSystem
    {
        private readonly IInputController<MainLayer> _inputController;
        public event Action OnInput;
        public Vector2 Position { get; private set; }
        public TouchPhase Phase { get; private set; }
        
        private const int _mouseButton = 0;
        private const float _minInputDistance = 0.05f;
        private readonly float _diagonalLength;
        private bool _onceInvoke = true;

        
        public HybridInputSystem(IInputController<MainLayer> inputController)
        {
            _inputController = inputController;
            _diagonalLength = new Vector2(Screen.width, Screen.height).magnitude;
        }

        public void Tick()
        {
            if (_inputController.Locked)
            {
                return;
            }

            if (Input.GetMouseButtonDown(_mouseButton))
            {
                Phase = TouchPhase.Began;
                Position = Input.mousePosition;
                _onceInvoke = false;
                OnInput?.Invoke();
            }
            else if (Input.GetMouseButton(_mouseButton))
            {
                Vector2 currInput = Input.mousePosition;
                var currentDir = Position - currInput;
                
                if (currentDir.magnitude / _diagonalLength >= _minInputDistance)
                {
                    Phase = TouchPhase.Moved;
                    Position = Input.mousePosition;
                    _onceInvoke = false;
                    OnInput?.Invoke();
                }
                else if(!_onceInvoke)
                {
                    Phase = TouchPhase.Stationary;
                    Position = Input.mousePosition;
                    _onceInvoke = true;
                    OnInput?.Invoke();
                }
            } 
            else if (Input.GetMouseButtonUp(_mouseButton))
            {
                Phase = TouchPhase.Ended;
                Position = Input.mousePosition;
                OnInput?.Invoke();
            }
            else if(!_onceInvoke)
            {
                Phase = TouchPhase.Canceled;
                Position = Vector2.zero;
                _onceInvoke = true;
                OnInput?.Invoke();
            }
        }
    }

    public class InputRaycaster
    {
        private readonly HybridInputSystem _inputSystem;
        private readonly FarmCameraSceneObject _farmCameraSceneObject;

        public InputRaycaster(FarmCameraSceneObject farmCameraSceneObject,
                              HybridInputSystem inputSystem)
        {
            _farmCameraSceneObject = farmCameraSceneObject;
            _inputSystem = inputSystem;
        }
    }
}