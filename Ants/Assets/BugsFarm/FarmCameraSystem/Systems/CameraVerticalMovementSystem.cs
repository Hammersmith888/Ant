using System;
using System.Collections.Generic;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.InputService;
using DG.Tweening;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.FarmCameraSystem
{
    public class CameraVerticalMovementSystem : IInitializable, IDisposable
    {
        public event Action OnMove;
        public event Action OnCompleted;
        
        private readonly string _nameKey;
        private readonly ISwipeSystem _swipeSystem;
        private readonly IInstantiator _instantiator;
        private readonly Transform _cameraTransform;
        private readonly CameraVerticalMovementModel _model;
        private readonly PrefabLoader _prefabLoader;

        private bool _inMove;
        private Vector2? _upTargetSwipe;
        private Vector2? _downTargetSwipe;
        private List<Transform> _pathTargets;
        private Sequence _currenAnimation;
        
        public CameraVerticalMovementSystem(Camera camera,
                                            CameraVerticalMovementModel model,
                                            ISwipeSystem swipeSystem, 
                                            IInstantiator instantiator)
        {
            _cameraTransform = camera.transform;
            _swipeSystem = swipeSystem;
            _instantiator = instantiator;
            _model = model;
        }
        public void Initialize()
        {
            _pathTargets = new List<Transform>();
            foreach (var obj in _model.Targets)
            {
                var target = _instantiator.InstantiatePrefab(obj);
                _pathTargets.Add(target.transform);
            }
            _swipeSystem.OnSwipe += OnSwipe;
            SetupTargets();
        }
        public void Dispose()
        {
            _swipeSystem.OnSwipe -= OnSwipe;
            var targets = _pathTargets.ToArray();
            foreach (var target in targets)
            {
                if(target)
                {
                    Object.Destroy(target.gameObject);
                }
            }
            _pathTargets.Clear();
        }
        public void Move(Vector3 worldPosition)
        {
            _cameraTransform.DOKill();
            _currenAnimation?.Kill();
            
            worldPosition = worldPosition.SetXZ(_cameraTransform.position);
            _currenAnimation = DOTween.Sequence();
            _currenAnimation.Append(_cameraTransform.DOMove(worldPosition, _model.AnimationSetup.durationIn))
                .SetEase(_model.AnimationSetup.easeIn)
                .SetDelay(_model.AnimationSetup.delay)
                .OnComplete(OnMoveComplete)
                .Play();
        }
        private void OnSwipe(Vector2 dir)
        {
            if(!Filter(dir))
            {
                return;
            }

            if (dir == Vector2.up && _upTargetSwipe.HasValue)
            {
                dir = _upTargetSwipe.Value;
            }
            else if(dir == Vector2.down && _downTargetSwipe.HasValue)
            {
                dir = _downTargetSwipe.Value;
            }
            else
            {
                return;
            }
            Move(dir);
            _inMove = true;
            OnMove?.Invoke();
        }
        private bool Filter(Vector2 dir)
        {
            var toUp = dir == Vector2.up &&  _upTargetSwipe.HasValue;
            var toDown = dir == Vector2.down && _downTargetSwipe.HasValue;
			
            return (toUp || toDown) && !_inMove;
        }
        private void SetupTargets()
        {
            for (var i = 0; i < _pathTargets.Count; i++)
            {
                var wayPoint = _pathTargets[i];
                if (Mathf.Abs(_cameraTransform.position.y - wayPoint.position.y) > 1) 
                    continue;
					
                if (i + 1 < _pathTargets.Count)
                {
                    _upTargetSwipe = _pathTargets[i+1].position;
                }
                else
                {
                    _upTargetSwipe = null;
                }

                if (i - 1 >= 0)
                {
                    _downTargetSwipe = _pathTargets[i - 1].position;
                }
                else
                {
                    _downTargetSwipe = null;
                }
            }
        }
        private void OnMoveComplete()
        {
            SetupTargets();
            _inMove = false;
            _currenAnimation?.Kill();
            _cameraTransform.DOKill();
            OnCompleted?.Invoke();
        }
    }
}