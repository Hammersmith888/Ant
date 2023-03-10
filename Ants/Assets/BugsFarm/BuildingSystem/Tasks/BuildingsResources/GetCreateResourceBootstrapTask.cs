using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GetCreateResourceBootstrapTask : BaseTask
    {
        private readonly Vector2 _taskpoint;
        private readonly string _itemId;
        private readonly ResourceArgs _args;
        private readonly IInstantiator _instantiator;
        private ITask _taskProcessed;

        private GetCreateResourceBootstrapTask(Vector2 taskpoint,
                                               string itemId, 
                                               int itemCount,
                                               ResourceArgs args,
                                               IInstantiator instantiator)
        {
            _itemId = itemId;
            _args = args;
            _instantiator = instantiator;
            _taskpoint = taskpoint;
            GivesReward = new TaskParams(new TaskParamModel(TaskParamID.ItemID, itemId, itemCount.ToString()));
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
                _instantiator.Instantiate<MoveToScenePointTask>(new object[]  {_taskpoint, _args.WalkAnimKey}),
                _instantiator.Instantiate<RotateToTask>(new object[] {Tools.RandomBool()}),
                _instantiator.Instantiate<GetCreateResourceTask>(new object[] {_itemId, _args})
            };
            _taskProcessed = _instantiator.Instantiate<BootstrapTask>(new object[]{tasks});
            _taskProcessed.OnComplete  += _ => Completed();
            _taskProcessed.OnInterrupt += _ => Interrupt();
            _taskProcessed.Execute(args);
        }
        public override Vector2[] GetPositions()
        {
            return new[] {_taskpoint};
        }
        
        protected override void OnForceCompleted()
        {
            _taskProcessed?.ForceComplete();
            _taskProcessed = null;
        }
        protected override void OnInterrupted()
        {
            _taskProcessed?.Interrupt();
            _taskProcessed = null;
        }
        protected override void OnCompleted()
        {
            _taskProcessed = null;
        }
    }
}