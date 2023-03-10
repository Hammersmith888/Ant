using System;
using System.Linq;
using BugsFarm.Services.InputService;
using UnityEngine;
using Zenject;

namespace BugsFarm.FarmCameraSystem
{
    public class FarmCameraController : IInitializable, IDisposable, ICameraController
    {
        public Camera Camera => _view.Camera;
        public ICameraCinematicSystem Cinematic => _cameraCinematicSystem;

        private readonly IInputController<MainLayer> _inputController;
        private readonly FarmCameraSceneObject _view;
        private readonly IInstantiator _instantiator;
        
        private bool _allowConstraints;
        private bool _allowProCamera2D;
        private bool _allowFollowHorizontal;
        private bool _allowFollowVertical;

        private CameraVerticalMovementSystem _cameraVerticalMovementSystem;
        private CameraCinematicSystem _cameraCinematicSystem;
        private Camera2DSystem _camera2DSystem;
        private CameraPanSystem _cameraPanSystem;
        private CameraConstraintedSystem _cameraConstraintedSystem;
        
        public FarmCameraController(FarmCameraSceneObject view,
                                    IInstantiator instantiator,
                                    IInputController<MainLayer> inputController)
        {
            _view = view;
            _instantiator = instantiator;
            _inputController = inputController;
        }

        public void Initialize()
        {
            var args = new object[] {Camera};
            _cameraVerticalMovementSystem = _instantiator.Instantiate<CameraVerticalMovementSystem>(args);
            _cameraCinematicSystem        = _instantiator.Instantiate<CameraCinematicSystem>(args);
            _camera2DSystem               = _instantiator.Instantiate<Camera2DSystem>(args);
            _cameraPanSystem              = _instantiator.Instantiate<CameraPanSystem>(args);
            _cameraConstraintedSystem     = _instantiator.Instantiate<CameraConstraintedSystem>(args.Append(_cameraPanSystem));
            
            _cameraCinematicSystem.OnStart            += OnCinematicStart;
            _cameraCinematicSystem.OnComplete         += OnCinematicComplete;
            _cameraVerticalMovementSystem.OnMove      += OnMoveVertical;
            _cameraVerticalMovementSystem.OnCompleted += OnCompletedMoveVertical;

            _cameraVerticalMovementSystem.Initialize();
            _cameraConstraintedSystem.Initialize(); // важно инициализировать перед синиматиком, чтобы при открытии комнаты сообщение доходило первее чем синиматик стартанет
            
            _cameraCinematicSystem.Initialize();
            _camera2DSystem.Initialize();
            _cameraPanSystem.Initialize();

            _cameraPanSystem.CenterPanTargetOnCamera();
            _camera2DSystem.ResetMovement();
        }

        public void Dispose()
        {            
            _cameraCinematicSystem.OnStart            -= OnCinematicStart;
            _cameraCinematicSystem.OnComplete         -= OnCinematicComplete;
            _cameraVerticalMovementSystem.OnMove      -= OnMoveVertical;
            _cameraVerticalMovementSystem.OnCompleted -= OnCompletedMoveVertical;
            _cameraVerticalMovementSystem.Dispose();
            _cameraCinematicSystem.Dispose();
            _camera2DSystem.Dispose();
            _cameraPanSystem.Dispose();
            _cameraConstraintedSystem.Dispose();
        }
        
        private void OnMoveVertical()
        {
            _inputController.Lock();
            _allowConstraints = _cameraConstraintedSystem.AllowConstraints;
            _cameraConstraintedSystem.SetAllowConstraints(false);
            _allowProCamera2D = _camera2DSystem.Enabled;
            _camera2DSystem.Enable(false);
        }
        private void OnCompletedMoveVertical()
        {
            _inputController.UnLock();
            _cameraConstraintedSystem.SetAllowConstraints(_allowConstraints);
            _cameraPanSystem.CenterPanTargetOnCamera();
            _camera2DSystem.Enable(_allowProCamera2D);
            _camera2DSystem.ResetMovement();
        }
        
        private void OnCinematicStart()
        {
            _inputController.Lock();
            
            _allowProCamera2D = _camera2DSystem.Enabled;
            _camera2DSystem.Enable(true);
            
            _allowFollowHorizontal = _camera2DSystem.FollowedHorizontal;
            _allowFollowVertical   = _camera2DSystem.FollowedVertical;
            
            _camera2DSystem.FollowHorizontal(true);
            _camera2DSystem.FollowVertical(true);
        }
        private void OnCinematicComplete()
        {
            _inputController.UnLock();
            
            _camera2DSystem.FollowHorizontal(_allowFollowHorizontal);
            _camera2DSystem.FollowVertical(_allowFollowVertical);
            
            _cameraPanSystem.CenterPanTargetOnCamera();
            _camera2DSystem.ResetMovement();
        }
    }
}