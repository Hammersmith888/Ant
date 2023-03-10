using BugsFarm.AnimationsSystem;
using BugsFarm.InventorySystem;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class ConsumeUnitTask : BaseTask
    {
        public override bool Interruptible => false;
        private readonly AnimatorStorage _animatorStorage;
        private readonly InventoryStorage _inventoryStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly NeedStatController _statController;
        private readonly string _itemId;

        private readonly AnimKey _animKey;
        private ISpineAnimator _animator;
        private string _unitId;
        private int _consumeCount;

        public ConsumeUnitTask(string itemId,
                               AnimKey animKey,
                               NeedStatController statController,
                               AnimatorStorage animatorStorage,
                               InventoryStorage inventoryStorage,
                               ISimulationSystem simulationSystem)
        {
            _animatorStorage = animatorStorage;
            _inventoryStorage = inventoryStorage;
            _simulationSystem = simulationSystem;
            _statController = statController;
            _itemId = itemId;
            _animKey = animKey;
            var needCount = ((int)_statController.NeedCount).ToString();
            Requirements = new TaskParams(new TaskParamModel(TaskParamID.ItemID, itemId, needCount));
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);
            _unitId = (string)args[0];
            _consumeCount = (int) args[1];
            _simulationSystem.OnSimulationStart += OnSimulationStart;
            
            if (_simulationSystem.Simulation)
            {
                _statController.RestockStart();
                OnSimulationStart();
                return;
            }
            _animator = _animatorStorage.Get(_unitId);
            _animator.OnAnimationComplete += OnAnimationComplete;
            _statController.RestockStart();
            _animator.SetAnim(_animKey);
        }

        private void Transaction()
        {
            if (!IsRunned || IsCompleted)
            {
                return;
            }
            
            _inventoryStorage.Get(_unitId).Remove(_itemId, _consumeCount);
            _statController.Update(_consumeCount);
            Completed();
        }
        
        private void OnSimulationStart()
        {
            Transaction();
        }
        
        private void OnAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned || 
                _animKey != animKey || 
                _simulationSystem.Simulation)
            {
                return;
            }
            
            Transaction();
        }

        protected override void OnDisposed()
        {
            if(IsExecuted)
            {
                if (_animator != null)
                {
                    _animator.OnAnimationComplete -= OnAnimationComplete;
                }
                _simulationSystem.OnSimulationStart -= OnSimulationStart;
            }
            
            _statController.RestockEnd();
            _animator = null;
        }
    }
}