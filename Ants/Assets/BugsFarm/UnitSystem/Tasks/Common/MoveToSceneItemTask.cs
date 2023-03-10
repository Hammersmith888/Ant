using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.TaskSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class MoveToSceneItemTask : BaseTask
    {
        private readonly AnimKey _walkAnim;
        private readonly string _buildingGuid;
        private readonly bool _rotateToPoint;
        private readonly IPosSide _point;
        private readonly IInstantiator _instantiator;
        private IDisposable _moveEvent;
        private ITask _taskProcessed;
        private object[] _args;

        public MoveToSceneItemTask(string buildingGuid,
                                   IPosSide point,
                                   IInstantiator instantiator,
                                   bool rotateToPoint = true,
                                   AnimKey walkAnim = AnimKey.Walk)
        {
            _buildingGuid = buildingGuid;
            _rotateToPoint = rotateToPoint;
            _point = point;
            _instantiator = instantiator;
            _walkAnim = walkAnim;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(_args = args);
            _moveEvent = MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(OnSceneItemMoved);
            CreateMoveTask();
        }

        public override Vector2[] GetPositions()
        {
            return _point?.ToPositions() ?? new Vector2[0];
        }

        private void CreateMoveTask()
        {
            if (!IsRunned || IsCompleted) return;
            if (_point == null)
            {
                Debug.LogError($"{this} : Target point missed");
                Interrupt();
                return;
            }

            _taskProcessed?.Interrupt();
            var tasks = new List<ITask>
            {
                _instantiator.Instantiate<MoveToScenePointTask>(new object[] {_point.Position, _walkAnim}),
            };

            if (_rotateToPoint)
            {
                tasks.Add(_instantiator.Instantiate<RotateToTask>(new object[] {_point.LookLeft}));
            }

            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new object[] {tasks});
            _taskProcessed.OnComplete += _ => Completed();
            _taskProcessed.OnForceComplete += _ => ForceComplete();
            _taskProcessed.Execute(_args);
        }

        private void OnSceneItemMoved(PlaceBuildingProtocol protocol)
        {
            if (!IsRunned || IsCompleted || _buildingGuid != protocol.Guid || _point == null) return;

            CreateMoveTask();
        }
        
        protected override void OnDisposed()
        {
            _moveEvent?.Dispose();
            _moveEvent = null;
            _taskProcessed?.Interrupt();
            _taskProcessed = null;
            _args = null;
        }

        protected override void OnForceCompleted()
        {
            _taskProcessed?.ForceComplete();
            base.OnForceCompleted();
        }
    }
}