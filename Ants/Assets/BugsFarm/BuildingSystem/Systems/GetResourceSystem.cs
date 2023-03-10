using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.InventorySystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GetResourceSystem
    {
        /// <summary>
        /// Если ресурс был обновлен arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnResourceChanged;
        
        /// <summary>
        /// Если ресурс был истощен arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnResourceDepleted;
        
        /// <summary>
        /// Если были изменения в системе arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnSystemUpdate;
        
        private readonly IInstantiator _instantiator;
        private readonly TaskStorage _taskStorage;

        /// <summary>
        /// arg1 = Иденификатор предмета, arg2 = связанные с предметом контроллеры выдачи ресурсов 
        /// </summary>
        private readonly Dictionary<string, List<GetResourceController>> _items;
        /// <summary>
        /// arg1 = идентификатор контроллера, arg2 = Задачи связанные с контроллером
        /// </summary>
        private readonly Dictionary<string, List<ITask>> _createdTasks;

        public GetResourceSystem(IInstantiator instantiator, TaskStorage taskStorage)
        {
            _instantiator = instantiator;
            _taskStorage = taskStorage;
            _items = new Dictionary<string, List<GetResourceController>>();
            _createdTasks = new Dictionary<string, List<ITask>>();
        }
        public void Registration(GetResourceProtocol protocol)
        {
            if (Contains(protocol.ItemID, protocol.Guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Registration)} :: Resource with [ ItemID : {protocol.ItemID}, Guid : {protocol.Guid} ] alredy exist");
            }
            
            if(!HasItem(protocol.ItemID))
            {
                _items.Add(protocol.ItemID, new List<GetResourceController>());
            }
            
            var controller = _instantiator.Instantiate<GetResourceController>(new object[] {protocol});
            controller.OnDepleted += OnDepleted;
            controller.OnChanged  += OnChanged;
            _items[protocol.ItemID].Add(controller);
            CreateTasks(controller);
            OnSystemUpdate?.Invoke(protocol.ItemID, protocol.Guid);
        }
        public void UnRegistration(string itemId, string guid)
        {
            if(!Contains(itemId, guid)) return;
            
            var controller = _items[itemId].First(x=> x.OwnerGuid == guid);
            var controllers = _items[controller.ItemID];
            controllers.Remove(controller);
            if(controllers.Count == 0)
            {
                _items.Remove(controller.ItemID);
            }
            StopTasks(controller);
            controller.OnDepleted -= OnDepleted;
            controller.OnChanged  -= OnChanged;
            controller.Dispose();
            OnSystemUpdate?.Invoke(itemId, guid);
        }
        public bool Contains(string itemId, string buildingGuid)
        {
            return HasItem(itemId) && _items[itemId].Any(x=>x.OwnerGuid == buildingGuid);
        }   
        public int Count(string itemId)
        {
            return HasItem(itemId) ? _items[itemId].Sum(x=>x.ItemCount) : 0;
        }           
        public int Count(string itemId, string modelId)
        {
            return HasItem(itemId) ? _items[itemId].Where(x=>x.ModelID == modelId).Sum(x=>x.ItemCount) : 0;
        }   
        public IEnumerable<string> GetResourceOwners(string itemId)
        {
            return !HasItem(itemId) ? default : _items[itemId].Select(x=>x.OwnerGuid);
        }  
        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId);
        }  
        public bool HasItemExcludeGuids(string itemId, params string[] guids)
        {
            return HasItem(itemId) && (guids == null || guids.Length == 0 || _items[itemId].Any(x=> !guids.Contains(x.OwnerGuid)));
        }
        public bool HasItemExcludeModelIDs(string itemId, params string[] modelIds)
        {
            return HasItem(itemId) && (modelIds == null || modelIds.Length == 0 || _items[itemId].Any(x=> !modelIds.Contains(x.ModelID)));
        }
        public IEnumerable<IItem> GetImmediateItems(string itemId, ref int count, string ownerGuid)
        {
            if (!Contains(itemId, ownerGuid))
            {
                count = 0;
                return default;
            }
            var controller = _items[itemId].First(x => x.OwnerGuid == ownerGuid);
            return controller.GetImmediateItems(ref count);
        }
        private void CreateTasks(GetResourceController controller)
        {
            if(!IsValid(controller)) return;

            while (controller.CanCreateTask())
            {
                var args = new object[] {controller.PointsController.GetPoint(), controller};
                var resourceTask = (ITask)_instantiator.Instantiate(controller.TaskType, args);
                resourceTask.OnComplete  += task => OnTaskEnd(task, controller);
                resourceTask.OnInterrupt += task => OnTaskEnd(task, controller);
                
                if (!HasTasks(controller.Guid))
                {
                    _createdTasks.Add(controller.Guid, new List<ITask>());
                }
                
                _createdTasks[controller.Guid].Add(resourceTask);
                _taskStorage.DeclareTask(controller.OwnerGuid, controller.ModelID, resourceTask.GetName(), resourceTask, false);
            }
        }
        private void StopTasks(GetResourceController controller)
        {
            if(!IsValid(controller)) return;
            
            if (HasTasks(controller.Guid))
            {
                var copyTasks = _createdTasks[controller.Guid].ToArray();
                // first remove all task before interupt
                foreach (var task in copyTasks)
                {
                    _createdTasks[controller.Guid].Remove(task);
                    if (_createdTasks[controller.Guid].Count == 0)
                    {
                        _createdTasks.Remove(controller.Guid);
                    }
                    if (_taskStorage.HasTask(task.Guid))
                    {
                        _taskStorage.Remove(task.Guid);
                    }
                }

                foreach (var task in copyTasks)
                {
                    if(!task.IsCompleted)
                    {
                        task.Interrupt();
                    }
                }
            }
        }
        private void ClearTask(ITask task, GetResourceController controller)
        {
            if(!IsValid(controller)) return;
            if (!HasTasks(controller.Guid)) return;
            
            _createdTasks[controller.Guid].Remove(task);
            if (_createdTasks[controller.Guid].Count == 0)
            {
                _createdTasks.Remove(controller.Guid);
            }
            if (_taskStorage.HasTask(task.Guid))
            {
                _taskStorage.Remove(task.Guid);
            }

            if(!task.IsCompleted)
            {
                task.Interrupt();
            }
        }
        private bool HasTasks(string controllerGuid)
        {
            return _createdTasks.ContainsKey(controllerGuid);
        }
        
        private bool IsValid(GetResourceController controller)
        {
            return !controller.IsNullOrDefault() && !controller.IsDisabled;
        }
        
        private void OnTaskEnd(ITask task, GetResourceController controller)
        {
            ClearTask(task, controller);
            
            if(IsValid(controller) && HasItem(controller.ItemID) && _items[controller.ItemID].Contains(controller) && controller.CanCreateTask())
            {
                CreateTasks(controller);
            }
        }
        private void OnDepleted(GetResourceController controller)
        {
            if(!IsValid(controller)) return;
            OnResourceDepleted?.Invoke(controller.ItemID, controller.OwnerGuid);
        }
        private void OnChanged(GetResourceController controller)
        {
            if(!IsValid(controller)) return;
            OnResourceChanged?.Invoke(controller.ItemID, controller.OwnerGuid);
        }
    }
}