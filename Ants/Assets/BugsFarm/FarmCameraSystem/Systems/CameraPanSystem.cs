using System;
using BugsFarm.Services.InputService;
using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using Zenject;

namespace BugsFarm.FarmCameraSystem
{
    public class CameraPanSystem : IInitializable, IDisposable, ICameraPanSystem
    {
        private readonly Camera _camera;
        private readonly IInputController<SceneLayer> _inputSceneController;
        private readonly IInputController<MainLayer> _inputController;

        private ProCamera2DPanAndZoom _proCamera2DPan;
        public bool AllowPan { get; private set; }
        public CameraPanSystem(Camera camera, 
                               IInputController<SceneLayer> inputSceneController,
                               IInputController<MainLayer> inputController)
        {
            _camera = camera;
            _inputSceneController = inputSceneController;
            _inputController = inputController;
        }

        public void Initialize()
        {
            _proCamera2DPan = _camera.GetComponent<ProCamera2DPanAndZoom>();
            AllowPan = _proCamera2DPan.enabled;
            _inputController.LockChangedEvent  += OnInputSceneStateChanged;
            _proCamera2DPan.OnPanStarted += OnPanStarted;
            _proCamera2DPan.OnPanFinished += OnPanFinished;
        }

        public void Dispose()
        {
            _proCamera2DPan.OnPanStarted  -= OnPanStarted;
            _proCamera2DPan.OnPanFinished -= OnPanFinished;
            _inputController.LockChangedEvent  -= OnInputSceneStateChanged;
        }
        public void CenterPanTargetOnCamera()
        {
            _proCamera2DPan.CenterPanTargetOnCamera();
        }
        public void SetAllowPan(bool allowPan)
        {
            _proCamera2DPan.enabled = allowPan;
            AllowPan = allowPan;
        }
        
        private void OnPanStarted()
        {
            _inputSceneController.Lock();
        }

        private void OnPanFinished()
        {
            _inputSceneController.UnLock();
        }

        private void OnInputSceneStateChanged(object sender, EventArgs e)
        {
            if (_proCamera2DPan.IsPanning)
            {
                return;
            }

            if (!_inputController.Locked)
            {
                _proCamera2DPan.enabled = AllowPan;
            }
            else
            {
                AllowPan = _proCamera2DPan.enabled;
                _proCamera2DPan.enabled = false;
            }
        }
    }
}