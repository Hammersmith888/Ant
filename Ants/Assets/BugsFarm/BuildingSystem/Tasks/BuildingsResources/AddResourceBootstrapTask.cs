using System;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AddResourceBootstrapTask : BaseTask
    {
        protected IInstantiator Instantiator;
        protected AddResourceController ResourceController;
        private ITask _taskProcessed;
        private IPosSide _taskpoint;

        [Inject]
        private void Inject(IPosSide taskpoint,
                            AddResourceController resourceController,
                            IInstantiator instantiator)
        {
            ResourceController = resourceController;
            Instantiator = instantiator;
            _taskpoint = taskpoint;

            var itemID = ResourceController.ItemID;
            var itemCount = ResourceController.NeedItemCount.ToString();
            var guid = ResourceController.OwnerGuid;
            var modelID = ResourceController.ModelID;
            var emptyKey = string.Empty;
            Requirements = new TaskParams(new TaskParamModel(TaskParamID.WithoutGuid, emptyKey, guid),
                                          new TaskParamModel(TaskParamID.WithoutModelID, emptyKey, modelID),
                                          new TaskParamModel(TaskParamID.ItemID, itemID, itemCount));
            OnInitialized();
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
                Instantiator.Instantiate<AddResourceToSceneItemTask>(new object[]
                {
                    ResourceController, new Action<int>(OnResourceAdd)
                }),
            };

            _taskProcessed = Instantiator.Instantiate<BootstrapTask>(new object[] {tasks});
            _taskProcessed.OnComplete += _ => Completed();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }

        public override Vector2[] GetPositions()
        {
            return _taskpoint.ToPositions();
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
        }

        protected override void OnInterrupted()
        {
            _taskProcessed?.Interrupt();
        }

        protected virtual void OnInitialized()
        {
        }

        protected virtual void OnResourceAdd(int resourceCount)
        {
        }
    }
}