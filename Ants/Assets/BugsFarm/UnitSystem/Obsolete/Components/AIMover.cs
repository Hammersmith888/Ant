using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    public class AIMover : IPostLoadRestorable, IPostSpawnInitable, ITickable
    {
        public bool IsLadder => _current.Tag == 2;  // веменно хардкод номера тэга из AstarPath
        public bool IsJoint => _current.Tag == 5;   // веменно хардкод номера тэга из AstarPath
        public bool IsFlyable => _current.Tag == 6; // веменно хардкод номера тэга из AstarPath
        public int DefaultSortingLayer => SortingLayers.NameToSortingID("MiddleObjectsGround");
        public int TraversableTags => _unitPath.TraversableTags;
        public Vector2 Normal => _transform.up;
        public Vector2 Position => _transform.position;
        public event Action OnComplete;

    #region Serializable
        private INode _target;
        private INode _current;
        private INode _next;
        private bool _isWalking;
        private bool _lookLeft;
        private float _speedOveridedMem;
        private Vector3S _savedEuler;
    #endregion
        
        private readonly Transform _transform;
        private readonly UnitPath _unitPath;
        private readonly AntAnimator _animator;
        private readonly AntType _antType;
        private readonly CfgAIMovement _stats;
        private readonly SteeringTarget _steeringTarget;
        private readonly TickableManager _tickableManager;
        
        private const float _reachDistance = 0.1f;
        private float _currMovementSpeed;
        private bool _canMove = false;

        public AIMover(Transform transform, 
                       AntAnimator animator, 
                       TickableManager tickableManager)
        {
            _tickableManager = tickableManager;
            _transform = transform;
            _animator = animator;
            _steeringTarget = new SteeringTarget();
            _steeringTarget.Initialize(transform);
            _stats = Data_Ants.Instance.GetData(_antType).AIMovement;;
            //_unitPath = new UnitPath(transform, new AntTraversableProvider(_antType));
            _unitPath.OnPathCalculated += OnPathCalculated;
            //_antType = antType;
        }
        public void PostSpawnInit()
        {
            _lookLeft = IsLookSameDirection(Vector2.left);
        }
        public void PostLoadRestore()
        {
            SetLayer();
            SetRotation(_savedEuler, true);
            if (_isWalking)
            {
                if (_speedOveridedMem > 0)
                {
                    GoTarget(_target.Position, _speedOveridedMem);
                }
                else
                {
                    GoTarget(_target.Position);
                }
            }
            else
            {
                Stay();
            }
        }
        public void Tick()
        {
            if (SimulationOld.Raw && _isWalking && _canMove)
            {
                ReachTarget();
                return;
            }

            // вращение происходит все время, чтобы достич нормального положения после достижения цели.
            _transform.rotation = Quaternion.Slerp(_transform.rotation, _steeringTarget.Rotation,
                                                  SimulationOld.DeltaTime * _stats.RotationSpeed);
            if (!_canMove)
            {
                _transform.rotation = _steeringTarget.Rotation;
                _tickableManager.Remove(this);
                return;
            }

            _transform.position = Vector3.MoveTowards(_transform.position, _steeringTarget.Position,
                                                     SimulationOld.DeltaTime * _currMovementSpeed);

            if ((_steeringTarget.Position - _transform.position).magnitude < _reachDistance)
            {
                if (_unitPath.IsPathComplete)
                {
                    Stay();
                    OnComplete?.Invoke();
                    return;
                }

                _current = _next;
                _next = _unitPath.Next;
                SetLayer();
                _steeringTarget.SetRotation(GetRotationToPath(out var forceRotate));
                _steeringTarget.SetPosition(_next.Position);
                if (forceRotate) ForceRotate();
            }
        }
        public void Stay()
        {
            SetAnimOverride(AnimKey.Idle);
            _isWalking = false;
            _canMove = false;
            RestoreSpeed();
        }
        public void ReachTarget()
        {
            //Заменяются ноды для правильного расчета поворота.
            _next = _target;
            _current = _unitPath.BeforeEnd;
            
            _unitPath.ReachTarget();
            _steeringTarget.SetPosition(_target.Position);
            _steeringTarget.SetRotation(GetRotationToPath(out _));
            TeleportAgentToSteering();
            _current = _target;
            SetLayer();

            if (_unitPath.IsPathComplete)
            {
                Stay();
                OnComplete?.Invoke();
            }
        }
        public bool CanReachTarget(Vector3 target)
        {
            return false;
        }
        public void GoTarget(Vector3 endPoint, float speedOverride)
        {
            _currMovementSpeed = _speedOveridedMem = speedOverride;
            StartPath(endPoint);
        }
        public void GoTarget(Vector3 endPoint)
        {
            StartPath(endPoint);
            RestoreSpeed(); // восстановить скорость если предыдущий путь был прерван.
        }
        public void SetPosition(INode node)
        {
            if (!node.IsNullOrDefault())
            {
                Debug.LogError($"{this} : Node does not exist!!!");
                return;
            }

            _current = _next = node;
            SetPosition(_current.Position);
        }
        public void SetPosition(Vector3 position)
        {
            _steeringTarget.SetPosition(position);
            TeleportAgentToSteering();
            SetLayer();
        }
        public void SetPositionAtClosedNode(Vector3 position)
        {
            var closedNode = GetNodeAtPoint(position);
            SetPosition(closedNode);
        }
        public void SetLookRandom()
        {
            SetLook(Tools.RandomBool());
        }
        public void SetLook(bool left)
        {
            var rotation = GetRotationToPath(out var forceRotate, left);
            SetRotation(rotation, forceRotate);
        }
        public void TeleportRandom()
        {
            var selected = GetRandomPosition();
            if (!selected.IsNullOrDefault())
            {
                SetPosition(selected);
            }
        }
        public void GoToRandom()
        {
            var selected = GetRandomPosition();
            if (!selected.IsNullOrDefault())
            {
                GoTarget(selected.Position);
            }
        }
        
        private void StartPath(Vector3 endPoint)
        {
            _isWalking = true;
            _canMove = false;
            if (SimulationOld.Raw)
            {
                var node = GetNodeAtPoint(endPoint);
                if (node.IsNullOrDefault())
                {
                    Debug.LogError($"{this} : Путь не удалось найти!!!");
                    return;
                }

                _unitPath.OverridePath(node, node);
                OnPathCalculated(true);
                return;
            }

            _unitPath.StartPath(endPoint);
        }
        private void SetRotation(Quaternion rotation, bool forceRotate)
        {
            _steeringTarget.SetRotation(rotation);
            if (forceRotate)
            {
                ForceRotate();
            }
        }
        private void SetRotation(Vector3 euler, bool forceUpdate)
        {
            SetRotation(Quaternion.Euler(euler), forceUpdate);
        }
        private void ForceRotate()
        {
            _transform.rotation = _steeringTarget.Rotation;
        }
        private void RestoreSpeed()
        {
            _speedOveridedMem = 0;
            _currMovementSpeed = _stats.MovementSpeed;
        }
        private void TeleportAgentToSteering()
        {
            _transform.position = _steeringTarget.Position;
            _transform.rotation = _steeringTarget.Rotation;
        }
        private INode GetRandomPosition()
        {
            return default;
            //return AstarTools.GetRandomNode(_antType, _unitPath.TraversableTags);
        }
        private INode GetNodeAtPoint(Vector3 point)
        {
            return default;
            //return AstarTools.GetNearestNodeAtPoint(_antType, point, _unitPath.TraversableTags);
        }
        private Quaternion GetRotationToPath(out bool forceRotate, bool? overrideLookLeft = null)
        {
            if (IsLadder)
            {
                forceRotate = false;
                return _transform.rotation;
            }

            var dir = _next.Position - _current.Position;
            if (!IsLookSameDirection(dir) || overrideLookLeft.HasValue)
            {
                _lookLeft = overrideLookLeft ?? !_lookLeft;
                forceRotate = true;
            }
            else
            {
                forceRotate = false;
            }

            var normal = _next.Normal;
            var surfaceNormal = Vector3.up;// _stats.SurfaceRepeat ? normal : Vector3.up;

            var surfaceRotatation = Quaternion.FromToRotation(Vector2.up, surfaceNormal);
            // Uinty при вращении на 180 - в данном случае вниз то, дополнительно вращается вокруг x оси на мгновение.
            // пока придумано так, если есть лучшее решение прошу улучшения)))
            if (surfaceRotatation.x == 1)
            {
                surfaceRotatation.x = 0;
                surfaceRotatation.z = 1;
            }

            var directionRotation =
                Quaternion.Euler(0, _lookLeft ? 180 : 0, 0); // Персонажи должены исходно смотреть вправо. 
            return surfaceRotatation * directionRotation;
        }
        /// <summary>
        /// Вернет true если агент смотрит в это направление.
        /// </summary>
        private bool IsLookSameDirection(Vector2 dir, uint angleMax = 120)
        {
            return Vector2.Angle(_transform.right, dir) < angleMax;
        }
        private void SetLayer()
        {
            if (!IsJoint)
            {
                _animator.Update(IsLadder, IsFlyable);
            }

            //_animator.SetSortingLayer(_current.Layer);
        }
        private void SetAnimOverride(AnimKey? anim = null)
        {
            if (anim.HasValue)
            {
                _animator.SetAnim(anim.Value);
            }
        }
        private void OnPathCalculated(bool calculcated)
        {
            if (calculcated)
            {
                _target = _unitPath.End;
                _steeringTarget.TeleportToLinked();
                _canMove = true;
                _tickableManager.Add(this);
            }
            else
            {
                Debug.LogError($"{this} : Путь не правильно просчитан!!!");
                Stay();
            }
        }
    }
}