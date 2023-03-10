using System;
using BugsFarm.RoomSystem;
using BugsFarm.SimulationSystem;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.FarmCameraSystem
{
    public class CameraCinematicSystem : IInitializable, IDisposable, ICameraCinematicSystem
    {
        public event Action OnStart;
        public event Action OnTargetStay;
        public event Action OnComplete;

        private readonly IInstantiator _instantiator;
        private readonly ISimulationSystem _simulationSystem;
        private readonly RoomDtoStorage _roomDtoStorage;
        
        private readonly Camera _camera;
        private readonly CinematicsResolvedStatesStorage _cinematicsStatesStorage;
        private readonly CameraCinematicsModel _model;

        private ProCamera2DCinematics _cinematic;
        private CinematicPresset _presset;
        private IDisposable _openRoomEvent;


        public CameraCinematicSystem(Camera camera,
                                     CameraCinematicsModel model,
                                     IInstantiator instantiator,
                                     ISimulationSystem simulationSystem,
                                     RoomDtoStorage roomDtoStorage,
                                     CinematicsResolvedStatesStorage cinematicsStatesStorage)
        {
            _camera = camera;
            _model = model;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _roomDtoStorage = roomDtoStorage;
            _cinematicsStatesStorage = cinematicsStatesStorage;
        }

        public void Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
        }
        public void Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
            RemoveCinematic();
        }
        public void EndCinematic()
        {
            if (_cinematic && _presset)
            {
                _cinematic.Unpause();
            }
        }
        
        private void AddCinematic(string id, Object presset)
        {
            // Удалить предыдущий синематик.
            RemoveCinematic();
			
            // Добавить новый синематик.
            _cinematicsStatesStorage.Add(new CinematicsState(id));
            if(_simulationSystem.Simulation) return; // TODO : collect info frop GID UI in simulation about this skip
            
            _presset = _instantiator.InstantiatePrefabForComponent<CinematicPresset>(presset);
            _cinematic = _camera.gameObject.AddComponent<ProCamera2DCinematics>();
            
            if(!_presset.WaitUserActionToEnd)
            {
                _cinematic.OnCinematicFinished.AddListener(OnCinematicFinished);
            }

            _cinematic.OnCinematicTargetReached.AddListener(OnCinematicTargetReached);
            
            SetupCinematic();
            DOVirtual.DelayedCall(Mathf.Abs(_presset.DelayBeforStart), () => { OnStart?.Invoke();  _cinematic.Play(); });
        }
        private void RemoveCinematic()
        {
            if (_cinematic)
            {
                _cinematic.GetComponent<ProCamera2DNumericBoundaries>().enabled = true;
                _cinematic.OnCinematicFinished.RemoveListener(OnCinematicFinished);
                _cinematic.OnCinematicTargetReached.RemoveListener(OnCinematicTargetReached);
                Object.Destroy(_cinematic);
            }

            if (_presset)
            {
                Object.Destroy(_presset.gameObject);
            }
        }
        private void SetupCinematic()
        {
            if(!_cinematic || !_presset || _presset.Targets == null || _presset.Targets.Length == 0)
            {
                throw new InvalidOperationException();
            }

            foreach (var target in _presset.Targets)
            {
                _cinematic.AddCinematicTarget(target.TargetTransform
                                             ,target.EaseInDuration
                                             ,target.HoldDuration
                                             ,target.Zoom
                                             ,target.EaseType
                                             ,target.SendMessageName
                                             ,target.SendMessageParam);
            }
            _cinematic.UseLetterbox = _presset.UseLetterBox;
            _cinematic.LetterboxAmount = _presset.LetterBoxAmmout;
            _cinematic.LetterboxAnimDuration = _presset.LetterBoxDuration;
            _cinematic.LetterboxColor = _presset.LetterBoxColor;
            _cinematic.UseNumericBoundaries = _presset.UseNumetricBoundaries;
        }
        
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if(!_roomDtoStorage.HasEntity(protocol.Guid) || (_cinematic && _presset)) return;
            
            var id = _roomDtoStorage.Get(protocol.Guid).ModelID;
            var isSkip = _cinematicsStatesStorage.HasEntity(id) || !_model.HasCinematic(id);

            if (!isSkip)
            {
                AddCinematic(id, _model.GetCinematic(id));
            }
            else
            {
                RemoveCinematic();
            }
        }
        private void OnCinematicFinished()
        {
            if(!_cinematic || !_presset) return;
            RemoveCinematic();
            OnComplete?.Invoke();
        }
        private void OnCinematicTargetReached(int id)
        {
            var lastTarget = _presset.Targets.Length - 1 == id;
            if (_presset.WaitUserActionToEnd && lastTarget)
            {
                _cinematic.Pause();
                return;
            }
            if (!_presset.StayOnLastReachTarget || !lastTarget) 
                return;
			
            const float beforeCinematicEnd = 0.05f; // во избежания перехода
            var holdDuration = _presset.Targets[id].HoldDuration;
            var newHoldDuration = Mathf.Max(holdDuration - beforeCinematicEnd, 0);

            DOVirtual.DelayedCall(newHoldDuration, () =>
            {
                OnTargetStay?.Invoke();
                OnCinematicFinished();
            });
        }
    }
}