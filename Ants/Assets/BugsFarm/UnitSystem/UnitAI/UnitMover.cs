using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.Graphic;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.UnitSystem
{
    public class UnitMover : IInitializable, ITickable, IUnitMover
    {
        /// <summary>
        /// Находимся на пересечении соединения путей
        /// </summary>
        public bool IsJoint => (Current != null && Current.Tag == 5); // хардкод номера тэга из AstarPath

        public Vector2 Normal => Transform.up;
        public Vector2 Position => _dto.Position;
        public Vector3 Rotation => _dto.Rotation;
        public int TraversableTags => _unitPath.TraversableTags;

        public LocationLayer Layer { get; private set; } = new LocationLayer(_defaultLayer, _defaultLayerOrder);

        public string Id { get; private set; }

        public event Action OnComplete;

        protected ISimulationSystem SimulationSystem;
        protected ISpineAnimator Animator;
        protected Transform Transform;
        private UnitPath _unitPath;
        private SteeringTarget _steeringTarget;
        private ITickableManager _tickableManager;
        private PathHelper _pathHelper;
        private IUnitFallSystem _unitFallSystem;
        private IActivitySystem _activitySystem;
        private UnitSceneObjectStorage _sceneObjectStorage;
        private StatsCollectionStorage _statsCollectionStorage;
        private AnimatorStorage _animatorStorage;

        private const int _defaultLayerOrder = 0;
        private const string _defaultLayer = "MiddleObjectsGround";
        private const string _movementSpeedStatKey = "stat_movementSpeed";
        private const string _repeatSurfaceSpeedStatKey = "stat_repeatSurfaceSpeed";

        private UnitMoverDto _dto;
        private UnitSceneObject _view;

        protected INode Current;
        private INode _target;
        private INode _next;
        private bool _lookLeft;
        private float _movementSpeed;
        private float _surfaceRotationSpeed;
        private bool _surfaceRepeat;
        private bool _canMove;
        private bool _finalized;
        private bool _tickable;

        [Inject]
        private void Inject(UnitMoverDto dto,
                            ITickableManager tickableManager,
                            IUnitFallSystem unitFallSystem,
                            IActivitySystem activitySystem,
                            UnitSceneObjectStorage sceneObjectStorage,
                            StatsCollectionStorage statCollectionStorage,
                            AnimatorStorage animatorStorage,
                            ISimulationSystem simulationSystem,
                            PathHelper pathHelper)
        {
            Id = dto.Guid;
            _dto = dto;
            _sceneObjectStorage = sceneObjectStorage;
            _statsCollectionStorage = statCollectionStorage;
            _animatorStorage = animatorStorage;
            SimulationSystem = simulationSystem;
            _tickableManager = tickableManager;
            _pathHelper = pathHelper;
            _unitFallSystem = unitFallSystem;
            _activitySystem = activitySystem;
            _steeringTarget = new SteeringTarget();
        }

        public void Initialize()
        {
            if (_finalized) return;
            var statCollection = _statsCollectionStorage.Get(Id);
            _view = _sceneObjectStorage.Get(Id);

            Transform = _view.transform;
            Animator = _animatorStorage.Get(Id);

            _movementSpeed = statCollection.GetValue(_movementSpeedStatKey);
            _surfaceRepeat = statCollection.HasEntity(_repeatSurfaceSpeedStatKey);
            _surfaceRotationSpeed = _surfaceRepeat ? statCollection.GetValue(_repeatSurfaceSpeedStatKey) : 0;

            _steeringTarget.Initialize(Transform);
            _unitPath = new UnitPath(Transform, new UnitTraversableProvider(_dto.ModelID));
            _unitPath.OnPathCalculated += OnPathCalculated;
            _activitySystem.OnStateChanged += OnActivityStateChanged;
            if (!_dto.Position.IsNullOrDefault())
            {
                _steeringTarget.SetPosition(_dto.Position);
                _steeringTarget.SetRotation(Quaternion.Euler(_dto.Rotation));
                Layer = new LocationLayer(_defaultLayer, _defaultLayerOrder);
                TeleportAgentToSteering();
                var findQuery = PathHelperQuery.Empty(Position)
                                   .UseLimitationsCheck(_dto.ModelID)
                                   .UseTraversableCheck(TraversableTags)
                                   .ProjectPosition();
                _target = _next = Current = _pathHelper.GetNearestNodeAtPoint(findQuery);
                SetLayer();
            }
            _lookLeft = IsLookSameDirection(Vector2.left);
            Stay();
            OnInitialized();
        }
        
        public void Dispose()
        {
            if (_finalized)
            {
                return;
            }
            _finalized = true;
            _activitySystem.OnStateChanged -= OnActivityStateChanged;
            if (_tickable)
            {
                _tickableManager.Remove(this);  
            }

            _unitPath.Dispose();
            _steeringTarget = null;
            _tickableManager = null;
            SimulationSystem = null;
            _statsCollectionStorage = null;
            Transform = null;
            OnComplete = null;
            _unitPath = null;
            _dto = null;
            OnDisposed();
        }

        public bool IsLoseGround()
        {
            if (_finalized) return false;
            if (Current == null || !Current.Walkable || _next == null || !_next.Walkable)
            {
                _unitFallSystem.OnLoseGround(Id);
                return true;
            }
            return false;
        }
        
        public virtual void Tick()
        {
            if (_finalized || !_activitySystem.IsActive(Id)) return;
            if (SimulationSystem.Simulation)
            {
                if (_canMove)
                {
                    ReachTarget(); 
                }
                return;
            }

            if (IsLoseGround() || _unitFallSystem.IsFall(Id))
            {
                return;
            }

            if (_surfaceRepeat)
            {
                Transform.rotation = Quaternion.Slerp(Transform.rotation, _steeringTarget.Rotation,
                                                      SimulationSystem.DeltaTime * _surfaceRotationSpeed);
            }

            if (!_canMove)
            {
                return;
            }

            Transform.position = Vector3.MoveTowards(Transform.position, _steeringTarget.Position,
                                                     SimulationSystem.DeltaTime * _movementSpeed);

            if (_steeringTarget.Position == Transform.position)
            {
                if (!_canMove) return;
                if (_unitPath.IsPathComplete && _unitPath.End == Current)
                {
                    Stay();
                    UpdateDto();
                    OnComplete?.Invoke();
                    return;
                }
                
                Current = _next;
                _next = _unitPath.Next;
                var rotation = GetRotationToPath(out var forceRotate);
                _steeringTarget.SetRotation(rotation);
                _steeringTarget.SetPosition(_next.Position);
                if (forceRotate)
                {
                    SetRotation(rotation);
                }
                SetLayer();
                UpdateDto();
            }
        }

        public void Stay()
        {
            if (_finalized || !_activitySystem.IsActive(Id))
            {
                return;
            }
            _canMove = false;
            SetLayer();
        }

        public bool CanReachTarget(params Vector2[] targets)
        {
            var findQuery = PathHelperQuery.Empty(targets)
                .UseLimitationsCheck(_dto.ModelID)
                .UseTraversableCheck(TraversableTags);
            return !_finalized && _pathHelper.HasNearestNodeAtPoints(findQuery);
        }

        public void GoTarget(Vector2 endPoint)
        {
            if (_finalized || !_activitySystem.IsActive(Id) || _unitFallSystem.IsFall(Id))
            {
                return;
            }

            if (IsLoseGround())
            {
                return;
            }
            
            Stay(); // остановка предыдущего движения
            if (!CanReachTarget(endPoint))
            {
                if(_finalized) return;
                Debug.LogError($"{this} : Not reachable point : {endPoint}");
                UpdateDto();
                OnComplete?.Invoke();
                return;
            }
            
            if (SimulationSystem.Simulation)
            {
                var findQuery = PathHelperQuery.Empty(endPoint)
                    .UseLimitationsCheck(_dto.ModelID)
                    .UseTraversableCheck(TraversableTags);
                var node = _pathHelper.GetNearestNodeAtPoint(findQuery);
                if (node == null)
                {
                    if(_finalized) return;
                    Debug.LogError($"{this} : Путь в симуляции не удалось найти!!!");
                    OnComplete?.Invoke();
                    return;
                }

                _unitPath.OverridePath(node, node);
                ReachTarget();
                return;
            }
            
            _unitPath.StartPath(endPoint);
        }

        public void SetPosition(INode node)
        {
            if (_finalized) return;
            Current = _next = _target = node;
            if (Current != null)
            {
                SetPosition(Current.Position);   
            }
        }

        public void SetPosition(Vector2 position)
        {
            if (_finalized)
            {
                return;
            }
            _steeringTarget.SetPosition(position);
            Transform.position = position;
            SetLayer();
            UpdateDto();
        }

        public void SetRotation(Quaternion rotation)
        {
            if (_finalized) return;
            _steeringTarget.SetRotation(rotation);
            Transform.rotation = rotation;
            UpdateDto();
        }

        public void SetLook(bool left)
        {
            if (_finalized) return;
            SetRotation(GetRotationToPath(out _, left));
        }

        public void ReachTarget()
        {
            if(_finalized) return;
            
            Current = _next = _target = _unitPath.End;
            _unitPath.ReachTarget();
            if (_target.IsNullOrDefault())
            {
                Stay();
                UpdateDto();
                OnComplete?.Invoke();
                return;
            }
            _steeringTarget.SetRotation(GetRotationToPath(out _));
            _steeringTarget.SetPosition(_target.Position);
            TeleportAgentToSteering();
            Stay();
            UpdateDto();
            OnComplete?.Invoke();
        }

        private void TeleportAgentToSteering()
        {
            if (_finalized) return;
            Transform.position = _steeringTarget.Position;
            Transform.rotation = _steeringTarget.Rotation;
        }

        private void UpdateDto()
        {
            if (_finalized)
            {
                return;
            }

            if (!Transform)
            {
                return;
            }
            _dto.Position = Transform.position;
            _dto.Rotation = Transform.rotation.eulerAngles;
            _dto.Normal = Normal;
            _dto.Layer = Layer;
        }

        protected virtual Quaternion GetRotationToPath(out bool forceRotate, bool? overrideLookLeft = null)
        {
            if (_finalized)
            {
                forceRotate = false;
                return default;
            }

            if (_next == null || Current == null)
            {
                forceRotate = false;
                return Transform.rotation;
            }

            var dir = _next.Position - Current.Position;
            if (!IsLookSameDirection(dir) || overrideLookLeft.HasValue)
            {
                _lookLeft = overrideLookLeft ?? !_lookLeft;
                forceRotate = true;
            }
            else
            {
                forceRotate = false;
            }

            var normal = _next.Normal == Vector2.zero ? (Vector2)Transform.up : _next.Normal;
            var surfaceNormal = _surfaceRepeat ? normal : Vector2.up;
            var surfaceRotatation = Quaternion.FromToRotation(Vector2.up, surfaceNormal);

            // Uinty при вращении на 180 - в данном случае вниз то, дополнительно вращается вокруг x оси на мгновение.
            // пока придумано так, если есть лучшее решение, смело изменяй.
            if (surfaceRotatation.x == 1)
            {
                surfaceRotatation.x = 0;
                surfaceRotatation.z = 1;
            }

            // Персонажи должyы изначально смотреть вправо. 
            var directionRotation = Quaternion.Euler(0, _lookLeft ? 180 : 0, 0);
            return surfaceRotatation * directionRotation;
        }

        /// <summary>
        /// Вернет true если юнит смотрит в это направление.
        /// </summary>
        private bool IsLookSameDirection(Vector2 dir, uint angleMax = 120)
        {
            if (_finalized) return false;
            return Vector2.Angle(Transform.right, dir) < angleMax;
        }

        private void SetLayer()
        {
            if (_finalized) return;
            
            if (!IsJoint)
            {
                var layerName = Current?.LayerName ?? _next?.LayerName ?? _target?.LayerName ?? _defaultLayer;
                Layer = new LocationLayer(layerName, _defaultLayerOrder);
                _view.SetLayer(Layer);
            }

            OnUpdateVisible();
        }

        private void OnPathCalculated(bool calculcated)
        {
            if (_finalized) return;
            if (calculcated)
            {
                _target = _unitPath.End;
                Current = _unitPath.Next;
                _next = _unitPath.Next;
                var rotation = GetRotationToPath(out var forceRotate);
                _steeringTarget.SetRotation(rotation);
                _steeringTarget.SetPosition(_next.Position);
                if (forceRotate)
                {
                    SetRotation(rotation);
                }
                SetLayer();
                UpdateDto();
                _canMove = true;
            }
            else
            {
                Debug.LogError($"{this} : Путь не правильно просчитан!!!");
                Stay();
                OnComplete?.Invoke();
            }
        }
        
        private void OnActivityStateChanged(string entityId)
        {
            if (_finalized || entityId != Id)
            {
                return;
            }
            var active = _activitySystem.IsActive(entityId);
            if (active && !_tickable)
            {
                _tickable = true;
                _tickableManager.Add(this);
            }
            else if(!active && _tickable)
            {
                _tickable = false;
                _tickableManager.Remove(this);
            }
        }

        protected virtual void OnUpdateVisible()
        {
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnDisposed()
        {
        }
    }
}