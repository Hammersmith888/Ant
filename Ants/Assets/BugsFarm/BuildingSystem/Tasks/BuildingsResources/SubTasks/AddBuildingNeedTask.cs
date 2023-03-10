using BugsFarm.SimulatingSystem;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AddBuildingNeedTask : AddResourceBootstrapTask
    {
        private NeedStatController _needStatController;

        [Inject]
        private void Inject(NeedStatController needStatController)
        {
            _needStatController = needStatController;
        }

        public override void Execute(params object[] args)
        {
            if(IsExecuted) return;
            if (SimulatingCenter.IsSimulating)
            {
                IsExecuted = true;
                OnForceCompleted();
            }
            _needStatController.RestockStart();
            base.Execute(args);
        }

        protected override void OnResourceAdd(int resourceCount)
        {
            if(!IsRunned) return;
            _needStatController.Update(resourceCount);
        }
        
        protected override void OnForceCompleted()
        {
            if (IsExecuted)
            {
                var needItemCount = ResourceController.NeedItemCount;
                ResourceController.AddItem(ref needItemCount);
                _needStatController.Update(needItemCount);
                _needStatController.RestockEnd();
            }

            base.OnForceCompleted();
        }
        protected override void OnInterrupted()
        {
            if (IsExecuted)
            {
                _needStatController.RestockEnd();
            }
            base.OnInterrupted();
        }
        protected override void OnCompleted()
        {
            if (IsExecuted)
            {
                _needStatController.RestockEnd();
            }
            base.OnCompleted();
        }
    }
}