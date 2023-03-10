using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class SpawnUnitTask : BaseTask
    {
        public override bool Interruptible => false;

        private Vector2 _spawnPoint;
        private PathHelper _pathHelper;
        private UnitMoverStorage _moverStorage;
        private  UnitDtoStorage _unitDtoStorage;
        private UnitSceneObjectStorage _unitSceneObjectStorage;
        protected UnitSceneObject View;
        protected string UnitId;

        [Inject]
        private void Inject(Vector2 spawnPoint,
                            PathHelper pathHelper,
                            UnitMoverStorage moverStorage,
                            UnitDtoStorage unitDtoStorage,
                            UnitSceneObjectStorage unitSceneObjectStorage)
        {
            _spawnPoint = spawnPoint;
            _pathHelper = pathHelper;
            _moverStorage = moverStorage;
            _unitDtoStorage = unitDtoStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }

            base.Execute(args);
            UnitId = (string) args[0];
            var mover = _moverStorage.Get(UnitId);
            var dto = _unitDtoStorage.Get(UnitId);
            var findQuery = PathHelperQuery.Empty(_spawnPoint)
                .UseLimitationsCheck(dto.ModelID)
                .UseTraversableCheck(mover.TraversableTags)
                .ProjectPosition();
            var startNode = _pathHelper.GetNearestNodeAtPoint(findQuery);
            View = _unitSceneObjectStorage.Get(UnitId);
            if (startNode == null)
            {
                mover.SetPosition(null);
                mover.SetPosition(_spawnPoint);
            }
            else
            {
                mover.SetPosition(startNode);
                mover.SetPosition(_spawnPoint); // fake position override node position
            }

            mover.Stay();
            OnSpawned();
        }

        protected virtual void OnSpawned()
        {
            Completed();
        }

        protected override void OnDisposed()
        {
            _pathHelper = null;
            _moverStorage = null;
            UnitId = null;
            base.OnDisposed();
        }
    }
}