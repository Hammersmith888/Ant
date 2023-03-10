using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GetResourceBootstrapTask : BaseTask
    {
        protected IInstantiator Instantiator;
        protected GetResourceController ResourceController;
        private ITask _taskProcessed;
        private IPosSide _taskpoint;

        [Inject]
        private void Inject(IPosSide taskpoint,
                            GetResourceController resourceController,
                            IInstantiator instantiator)
        {
            _taskpoint = taskpoint;
            ResourceController = resourceController;
            Instantiator = instantiator;
            var itemID = ResourceController.ItemID;
            var itemCount = ResourceController.ItemCount.ToString();
            GivesReward = new TaskParams(new TaskParamModel(TaskParamID.ItemID, itemID, itemCount));
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }

            base.Execute(args);
            var tasks = new ITask[]
            {
                Instantiator.Instantiate<MoveToSceneItemTask>(new object[]
                {
                    ResourceController.OwnerGuid, _taskpoint, ResourceController.Args.WalkAnimKey
                }),
                Instantiator.Instantiate<GetResourceFromSceneItemTask>(new object[] {ResourceController})
            };
            _taskProcessed = Instantiator.Instantiate<BootstrapTask>(new object[] {tasks});
            _taskProcessed.OnComplete += _ => Completed();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }

        public override Vector2[] GetPositions()
        {
            return _taskpoint?.ToPositions() ?? new Vector2[0];
        }

        protected override void OnDisposed()
        {
            ResourceController.Release();
            _taskProcessed = null;
            _taskpoint = null;
            ResourceController = null;
            Instantiator = null;
            base.OnDisposed();
        }

        protected override void OnForceCompleted()
        {
            _taskProcessed?.ForceComplete();
            base.OnForceCompleted();
        }

        protected override void OnInterrupted()
        {
            _taskProcessed?.Interrupt();
            base.OnInterrupted();
        }
    }
}