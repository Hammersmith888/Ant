using System;
using BugsFarm.AstarGraph;
using BugsFarm.Graphic;
using BugsFarm.Services.StatsService;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitLarvaMover : IUnitMover, IInitializable
    {
        /// <summary>
        /// Находимся на пересечении соединения путей
        /// </summary>
        public bool IsJoint => (_current != null && _current.Tag == 5); // хардкод номера тэга из AstarPath

        public Vector2 Normal => _transform.up;
        public Vector2 Position => _dto.Position;
        public Vector3 Rotation => _dto.Rotation;
        public int TraversableTags => -1;

        public LocationLayer Layer { get; private set; } = new LocationLayer(_defaultLayer, _defaultLayerOrder);
        public string Id => _dto.Guid;
        public event Action OnComplete;

        private readonly PathHelper _pathHelper;
        private readonly UnitSceneObjectStorage _viewStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitMoverDto _dto;

        private const string _repeatSurfaceSpeedStatKey = "stat_repeatSurfaceSpeed";
        private const int _defaultLayerOrder = 0;
        private const string _defaultLayer = "MiddleObjectsGround";

        private UnitSceneObject _view;
        private INode _current;
        private INode _target;
        private Transform _transform;
        private bool _surfaceRepeat;
        private bool _lookLeft;

        public UnitLarvaMover(UnitMoverDto dto,
                              PathHelper pathHelper,
                              UnitSceneObjectStorage viewStorage,
                              StatsCollectionStorage statsCollectionStorage)
        {
            _dto = dto;
            _pathHelper = pathHelper;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void Initialize()
        {
            var statCollection = _statsCollectionStorage.Get(Id);
            _view = _viewStorage.Get(Id);
            _transform = _view.transform;
            _surfaceRepeat = statCollection.HasEntity(_repeatSurfaceSpeedStatKey);

            if (_dto.Position != Vector3.positiveInfinity)
            {
                _transform.position = _dto.Position;
                _transform.rotation = Quaternion.Euler(_dto.Rotation);
                Layer = new LocationLayer(_defaultLayer, _defaultLayerOrder);
                var findQuery = PathHelperQuery.Empty(Position)
                    .UseLimitationsCheck(_dto.ModelID)
                    .UseTraversableCheck(TraversableTags);
                _target = _current = _pathHelper.GetNearestNodeAtPoint(findQuery);
                SetLayer();
            }
            _lookLeft = IsLookSameDirection(Vector2.left);
            Stay();
        }

        public void Dispose()
        {
            _transform = null;
        }

        public void Stay()
        {
            SetLayer();
            UpdateDto();
        }

        public bool CanReachTarget(params Vector2[] targets)
        {
            var findQuery = PathHelperQuery.Empty(Position)
                .UseLimitationsCheck(_dto.ModelID)
                .UseTraversableCheck(TraversableTags);
            return targets != null && _pathHelper.HasNearestNodeAtPoints(findQuery);
        }

        public bool IsLoseGround()
        {
            return (_current.IsNullOrDefault() || !_current.Walkable || _target.IsNullOrDefault() || !_target.Walkable);
        }

        public void ReachTarget()
        {
            throw new NotImplementedException();
        }

        public void GoTarget(Vector2 endPoint)
        {
            var findQuery = PathHelperQuery.Empty(Position)
                .UseLimitationsCheck(_dto.ModelID)
                .UseTraversableCheck(TraversableTags)
                .ProjectPosition();
            _target = _pathHelper.GetNearestNodeAtPoint(findQuery);
            if (_target == null)
            {
                Debug.LogError($"{this} : {nameof(GoTarget)} :: Target position cannot be reached : {endPoint}");
                return;
            }

            SetPosition(endPoint);
            SetRotation();
            OnComplete?.Invoke();
        }

        public void SetPosition(Vector2 position)
        {
            _transform.position = position;
            var findQuery = PathHelperQuery.Empty(Position)
                .UseLimitationsCheck(_dto.ModelID)
                .UseTraversableCheck(TraversableTags)
                .ProjectPosition();
            _current = _pathHelper.GetNearestNodeAtPoint(findQuery);
            SetLayer();
            UpdateDto();
        }

        public void SetPosition(INode node)
        {
            _current = node;
            _transform.position = _current.Position;
            SetLayer();
        }

        public void SetRotation(Quaternion rotation)
        {
            _transform.rotation = rotation;
            UpdateDto();
        }

        public void SetLook(bool left)
        {
            SetRotation(left);
        }

        private void SetLayer()
        {
            if (IsJoint || !_view)
                return;

            var layerID = _current?.LayerName ?? _target?.LayerName ?? _defaultLayer;
            Layer = new LocationLayer(layerID, _defaultLayerOrder);
            _view.SetLayer(Layer);
        }

        private void SetRotation(bool? overrideLookLeft = null)
        {
            if (_target == null || _current == null)
            {
                return;
            }

            var dir = _target.Position - _current.Position;
            if (!IsLookSameDirection(dir) || overrideLookLeft.HasValue)
            {
                _lookLeft = overrideLookLeft ?? !_lookLeft;
            }

            var normal = _target.Normal == Vector2.zero ? (Vector2)_transform.up : _target.Normal;
            var surfaceNormal = _surfaceRepeat ? normal : Vector2.up;

            var surfaceRotatation = Quaternion.FromToRotation(Vector2.up, surfaceNormal);
            // Uinty при вращении на 180 - в данном случае вниз то, дополнительно вращается вокруг x оси на мгновение.
            // пока придумано так, если есть лучшее решение прошу улучшения)))
            if (surfaceRotatation.x == 1)
            {
                surfaceRotatation.x = 0;
                surfaceRotatation.z = 1;
            }

            var directionRotation =
                Quaternion.Euler(0, _lookLeft ? 180 : 0, 0); // Персонажи должны исходно смотреть вправо. 
            _transform.rotation = surfaceRotatation * directionRotation;
            UpdateDto();
        }

        private void UpdateDto()
        {
            if (!_transform) return;
            _dto.Position = _transform.position;
            _dto.Rotation = _transform.rotation.eulerAngles;
            _dto.Normal = Normal;
            _dto.Layer = Layer;
        }

        private bool IsLookSameDirection(Vector2 dir, uint angleMax = 120)
        {
            return Vector2.Angle(_transform.right, dir) < angleMax;
        }
    }
}