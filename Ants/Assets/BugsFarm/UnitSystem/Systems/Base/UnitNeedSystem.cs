using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public abstract class UnitNeedSystem : IInitializable, IDisposable
    {
        /// <summary>
        /// Когда юнит нуждается в ресурсе arg1 = идентификатор юнита
        /// </summary>
        public event Action<string> OnHungry;

        protected abstract string PrefixKey { get; }
        protected abstract string ItemID { get; }
        protected abstract AnimKey ConsumeAnimKey { get; }
        protected abstract string ResourceStatKey { get; }
        protected abstract string NeedTimeStatKey { get; }
        protected abstract string NoNeedTimeStatKey { get; }
        
        private IInstantiator _instantiator;
        private UnitTaskProcessorStorage _taskProcessorsStorage;
        private GetResourceSystem _getResourceSystem;
        private TaskBuilderSystem _taskBuilderSystem;
        private TaskMock _consumeTaskMock;

        /// <summary>
        /// arg1 = Идентификатор юнита, arg2 = контроллер состояния портребности
        /// </summary>
        private Dictionary<string, NeedStatController> _storage;

        /// <summary>
        /// arg1 = идентификатор юнита
        /// </summary>
        private List<string> _consumeUnits;

        /// <summary>
        /// arg1 = идентификатор юнита
        /// </summary>
        private List<string> _hangryUnits;

        [Inject]
        private void Inject(IInstantiator instantiator,
                            UnitTaskProcessorStorage taskProcessorsStorage,
                            GetResourceSystem getResourceSystem,
                            TaskBuilderSystem taskBuilderSystem,
                            TaskStorage taskStorage)
        {
            _instantiator = instantiator;
            _taskProcessorsStorage = taskProcessorsStorage;
            _getResourceSystem = getResourceSystem;
            _taskBuilderSystem = taskBuilderSystem;
            _storage = new Dictionary<string, NeedStatController>();
            _consumeUnits = new List<string>();
            _hangryUnits = new List<string>();
        }

        public void Initialize()
        {
            _consumeTaskMock = _instantiator.Instantiate<TaskMock>(new object[] {nameof(ConsumeUnitTask), false});
            _getResourceSystem.OnSystemUpdate += OnGetSystemUpdate;
        }

        public void Dispose()
        {
            OnHungry = null;
            _getResourceSystem.OnSystemUpdate -= OnGetSystemUpdate;
            var units = _storage.Keys.ToArray();
            foreach (var unit in units)
            {
                UnRegistration(unit);
            }

            _storage.Clear();
            _hangryUnits.Clear();
            _consumeUnits.Clear();
        }

        public void Registration(string guid)
        {
            if (HasUnit(guid))
            {
                throw new
                    InvalidOperationException($"{this} : {nameof(Registration)} :: Unit with [Guid : {guid}], alredy exist.");
            }

            var controllerProtocol = new NeedStatPtotocol(guid, PrefixKey, ResourceStatKey, NoNeedTimeStatKey, NeedTimeStatKey, OnNeedRestock);
            var controller = _instantiator.Instantiate<NeedStatController>(new object[] {controllerProtocol});
            _storage.Add(guid, controller);
        }

        public void UnRegistration(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            var controller = _storage[guid];
            controller.OnNeed -= OnNeedRestock;
            controller.Dispose();
            _storage.Remove(guid);

            if (IsConsume(guid))
            {
                _consumeUnits.Remove(guid);

                if (_taskProcessorsStorage.HasEntity(guid))
                {
                    var taskProcessor = _taskProcessorsStorage.Get(guid);
                    taskProcessor.Interrupt();
                }
            }

            _hangryUnits.Remove(guid);
        }

        public bool HasUnit(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public bool IsHungry(string guid)
        {
            return _hangryUnits.Contains(guid);
        }

        public bool IsConsume(string guid)
        {
            return _consumeUnits.Contains(guid);
        }

        public INeedInfo GetInfo(string guid)
        {
            return HasUnit(guid) ? _storage[guid] : default;
        }

        public bool Start(string guid)
        {
            if (!HasUnit(guid)) return false;

            if (IsConsume(guid)) return false;

            if (!IsHungry(guid)) return false;

            if (!_taskProcessorsStorage.HasEntity(guid)) return false;

            var taskProcessor = _taskProcessorsStorage.Get(guid);
            if (!taskProcessor.CanInterrupt(_consumeTaskMock)) return false;

            var controller = _storage[guid];
            var args = new object[] {ItemID, ConsumeAnimKey, controller};
            var needTask = _instantiator.Instantiate<ConsumeUnitTask>(args);
            if (_taskBuilderSystem.Build(needTask, guid, out var buildedTask))
            {
                _consumeUnits.Add(guid);
                needTask.OnInterrupt += _ => OnTaskEnd(guid);
                needTask.OnComplete  += _ => OnTaskEnd(guid);
                taskProcessor.SetTask(buildedTask);
                return true;
            }

            if (controller.UseAvailable())
            {
                if(_hangryUnits.Contains(guid))
                {
                    _hangryUnits.Remove(guid);
                }
            }
            return false;
        }

        public void SetMax(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            var controller = _storage[guid];
            controller.Update(controller.NeedCount);
            controller.UseAvailable();
            if (IsConsume(guid))
            {
                var taskProcessor = _taskProcessorsStorage.Get(guid);
                taskProcessor.Interrupt();
            }
        }

        private void OnNeedRestock(string guid)
        {
            if (SimulatingCenter.IsSimulating)
                return;
            
            if(!HasUnit(guid)) return;
            
            if (!IsHungry(guid))
            {
                _hangryUnits.Add(guid);
            }
            
            OnHungry?.Invoke(guid);
        }

        private void OnGetSystemUpdate(string itemId, string buildingGuid)
        {
            if (itemId != ItemID || !_getResourceSystem.HasItem(itemId)) return;

            var units = _hangryUnits.ToArray();
            foreach (var guid in units)
            {
                if (!IsConsume(guid))
                {
                    OnHungry?.Invoke(guid);
                }
            }
        }
        
        private void OnTaskEnd(string guid)
        {
            if (!HasUnit(guid)) return;
            
            if (IsConsume(guid))
            {
                _consumeUnits.Remove(guid);
            }
            
            var controller = _storage[guid];
            if (!controller.IsNeed && IsHungry(guid))
            {
                _hangryUnits.Remove(guid);
            }
        }
    }
}