using System;
using System.Collections.Generic;
using BugsFarm.RoomSystem;
using BugsFarm.Services.UIService;
using Com.LuisPedroFonseca.ProCamera2D;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.FarmCameraSystem
{
    public class CameraConstraintedSystem : ICameraConstraintedSystem
    {
        public bool AllowConstraints => _pressets.Count <= 0 || (HasBoundaries() && _numericBoundaries.enabled);
        private readonly Camera _camera;
        private readonly CameraConstraintsModel _model;
        private readonly UIRoot _uiRoot;
        private readonly ICameraPanSystem _panSystem;
        private readonly IInstantiator _instantiator;
        private readonly RoomDtoStorage _roomDtoStorage;
        private ProCamera2DNumericBoundaries _numericBoundaries;
        private IDisposable _openRoomEvent;
        private readonly List<GameObject> _pressets;
        private string _currentID;
        
        public CameraConstraintedSystem(Camera camera,
                                        CameraConstraintsModel model,
                                        UIRoot uiRoot,
                                        ICameraPanSystem panSystem,
                                        IInstantiator instantiator,
                                        RoomDtoStorage roomDtoStorage)
        {
            _camera = camera;
            _model = model;
            _uiRoot = uiRoot;
            _panSystem = panSystem;
            _instantiator = instantiator;
            _roomDtoStorage = roomDtoStorage;
            _pressets = new List<GameObject>();
        }

        public void Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
            _numericBoundaries = _camera.GetComponent<ProCamera2DNumericBoundaries>();
        }
        
        public void Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
        }
        
        public void SetAllowConstraints(bool allow)
        {
            if(HasBoundaries())
            {
                _numericBoundaries.enabled = allow;
            }
        }

        private bool HasBoundaries()
        {
            if (!_numericBoundaries)
            {
                _numericBoundaries = _camera.GetComponent<ProCamera2DNumericBoundaries>();
            }

            return _numericBoundaries;
        }

        private void AutoFitLeftBoundaries(ConstraintsPresset pressets)
        {
            if(!pressets.AutoFitLeftBoundaries || _pressets.Count <= 0) return;
            var canvasScaler = _uiRoot.UICanvas.GetComponent<CanvasScaler>();
            
            const int maxDifferenceHeight = 420; // максимальный эталон разницы высоты 
            const float roundToIntPrecision = 100f; // для более точного сравнияния Aspect Ratio
            var etalonAspect = Mathf.RoundToInt( (canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y ) * roundToIntPrecision) ;
            var targetAspect = Mathf.RoundToInt(_camera.aspect * roundToIntPrecision);
            if(etalonAspect == targetAspect) return;
            
            var etalonHeight = canvasScaler.referenceResolution.y;
            var targetHeight = Screen.currentResolution.height;
            
            var deltaHeight = targetHeight - etalonHeight;
            var deltaSign = Mathf.Sign(deltaHeight);
            var diffirencePercent = (Mathf.Abs(deltaHeight) / maxDifferenceHeight) * 100f;
            
            foreach (var presset in _pressets)
            {
                if(!presset.TryGetComponent(out ProCamera2DTriggerBoundaries triggerBoundaries)){continue;}

                var min = pressets.AutoFitMin;
                var max = triggerBoundaries.LeftBoundary;
                var refBoundarySing = Mathf.Sign(max);
                var deltaBoundary = Mathf.Abs(min - max);
                var offsetBoundary = deltaBoundary * (diffirencePercent / 100f) * deltaSign;
                var targetBoundary = Mathf.Clamp(Mathf.Abs(max + offsetBoundary), Mathf.Abs(min), Mathf.Abs(max)) * refBoundarySing;
                
                triggerBoundaries.LeftBoundary = targetBoundary;
                triggerBoundaries.SetBoundaries();
            }
        }
        
        private void AddConstraints(string id, ConstraintsPresset pressets)
        {
            // Если состояние обновилось но, изменять нечего.
            if (_pressets.Count > 0 && _currentID == id)
                return;

            // Удалить предыдущие констраинты.
            RemoveConstraints();

            _panSystem.SetAllowPan(pressets.AllowPan);
            // Добавить новые констраинты.
            foreach (var prefab in pressets.Prefabs)
            {
                _pressets.Add(_instantiator.InstantiatePrefab(prefab));
            }
            
            AutoFitLeftBoundaries(pressets);

            _currentID = id;
        }
        
        private void RemoveConstraints()
        {
            if (_pressets.Count > 0)
            {
                while (_pressets.Count > 0)
                {
                    var presset = _pressets[0];
                    _pressets.RemoveAt(0);
                    Object.Destroy(presset);
                }
                _currentID = "-1";
            }
        }
        
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if(!_roomDtoStorage.HasEntity(protocol.Guid)) return;
            
            var roomDto = _roomDtoStorage.Get(protocol.Guid);
            var id = roomDto.ModelID;
            
            if(!_model.HasConstraint(id)) return;
            
            AddConstraints(id, _model.GetConstraints(id));
        }
    }
}