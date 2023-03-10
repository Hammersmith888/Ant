using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.ReloadSystem;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class AddResourceSystem
    {
        /// <summary>
        /// Если ресурс был обновлен arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnResourceChanged;

        /// <summary>
        /// Если ресурс был полон arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnResourceFull;

        /// <summary>
        /// Если были изменения в системе arg1 = идентификатор предмета, arg2 = иденификатор постройки
        /// </summary>
        public event Action<string, string> OnSystemUpdate;

        private readonly IInstantiator _instantiator;
        private readonly TaskStorage _taskStorage;

        /// <summary>
        /// arg1 = Иденификатор предмета, arg2 = связанные с предметом контроллеры выдачи ресурсов 
        /// </summary>
        private readonly Dictionary<string, List<AddResourceController>> _items;

        /// <summary>
        /// arg1 = идентификатор контроллера, arg2 = Задачи связанные с контроллером
        /// </summary>
        private readonly Dictionary<string, List<ITask>> _createdTasks;
        public AddResourceSystem(IInstantiator instantiator, TaskStorage taskStorage)
        {
            _instantiator = instantiator;
            _taskStorage = taskStorage;
            _items = new Dictionary<string, List<AddResourceController>>();
            _createdTasks = new Dictionary<string, List<ITask>>();
            MessageBroker.Default.Receive<RestockBuildingProtocol>().Subscribe(OnResourceRestockHandler);
        }

        public void Registration(AddResourceProtocol protocol)
        {
            if (Contains(protocol.ItemID, protocol.Guid))
            {
                throw new
                    InvalidOperationException($"{this} : {nameof(Registration)} :: Resource with [ ItemID : {protocol.ItemID}, Guid : {protocol.Guid} ] alredy exist");
            }

            if (!HasItem(protocol.ItemID))
            {
                _items.Add(protocol.ItemID, new List<AddResourceController>());
            }

            var controller = _instantiator.Instantiate<AddResourceController>(new object[] {protocol});
            controller.OnFull += OnFull;
            controller.OnChanged += OnChanged;
            _items[protocol.ItemID].Add(controller);
            CreateTasks(controller);
            OnSystemUpdate?.Invoke(protocol.ItemID, protocol.Guid);
        }

        public void UnRegistration(string itemId, string guid)
        {
            if (!Contains(itemId, guid)) return;

            var controller = _items[itemId].First(x => x.OwnerGuid == guid);
            var controllers = _items[itemId];
            controllers.Remove(controller);
            if (controllers.Count == 0)
            {
                _items.Remove(itemId);
            }

            StopTasks(controller);
            controller.OnFull -= OnFull;
            controller.OnChanged -= OnChanged;
            controller.Dispose();
            if (!GameReloader.IsReloading)
            {
                OnSystemUpdate?.Invoke(itemId, guid);
            }
        }

        public bool Contains(string itemId, string buildingGuid)
        {
            return HasItem(itemId) && _items[itemId].Any(x => x.OwnerGuid == buildingGuid);
        }

        public bool HasTasks(string itemId, string buildingGuid)
        {
            if (HasItem(itemId))
            {
                var controller = _items[itemId].FirstOrDefault(x => x.OwnerGuid == buildingGuid);
                return controller != null && _createdTasks.ContainsKey(controller.Guid);
            }

            return false;
        }

        public bool HasItem(string itemId)
        {
            return _items.ContainsKey(itemId);
        }

        public PointsController GetTaskPointController(string itemId, string buildingId)
        {
            if (!Contains(itemId, buildingId)) return default;
            return _items[itemId].First(x=>x.OwnerGuid == buildingId).PointsController;
        }

        private void CreateTasks(AddResourceController controller)
        {
            if (!IsValid(controller)) return;

            while (controller.CanCreateTask())
            {
                var extraArgs = controller.TaskExtraArgs.ToList();
                extraArgs.AddRange(new object[] {controller.PointsController.GetPoint(), controller});
                var resourceTask = (ITask) _instantiator.Instantiate(controller.TaskType, extraArgs);
                resourceTask.OnComplete += task => OnTaskEnd(task, controller);
                resourceTask.OnInterrupt += task => OnTaskEnd(task, controller);

                if (!HasControllerTasks(controller.Guid))
                {
                    _createdTasks.Add(controller.Guid, new List<ITask>());
                }

                _createdTasks[controller.Guid].Add(resourceTask);
                _taskStorage.DeclareTask(controller.OwnerGuid, controller.ModelID, resourceTask.GetName(), resourceTask,
                                         controller.TaskNotify);
            }
        }

        private void StopTasks(AddResourceController controller)
        {
            if (!IsValid(controller))
            {
                return;
            }

            if (HasControllerTasks(controller.Guid))
            {
                var copyTasks = _createdTasks[controller.Guid].ToArray();
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
                    if (!task.IsCompleted)
                    {
                        task.Interrupt();
                    }
                }
            }
        }

        private void ClearTask(ITask task, AddResourceController controller)
        {
            if (!IsValid(controller)) return;
            if (!HasControllerTasks(controller.Guid)) return;

            _createdTasks[controller.Guid].Remove(task);
            if (_createdTasks[controller.Guid].Count == 0)
            {
                _createdTasks.Remove(controller.Guid);
            }

            if (_taskStorage.HasTask(task.Guid))
            {
                _taskStorage.Remove(task.Guid);
            }

            if (!task.IsCompleted)
            {
                task.Interrupt();
            }
        }

        private bool HasControllerTasks(string controllerGuid)
        {
            return _createdTasks.ContainsKey(controllerGuid);
        }

        private bool IsValid(AddResourceController controller)
        {
            return !controller.IsNullOrDefault() && !controller.IsDisabled;
        }

        private void OnTaskEnd(ITask task, AddResourceController controller)
        {
            ClearTask(task, controller);

            if (IsValid(controller) && HasItem(controller.ItemID) && _items[controller.ItemID].Contains(controller) &&
                controller.CanCreateTask())
            {
                CreateTasks(controller);
            }
        }
        
        private void OnResourceRestockHandler(RestockBuildingProtocol protocol)
        {
            if (!HasTasks(protocol.ItemId, protocol.Guid))
            {
                return;
            }

            var controller = _items[protocol.ItemId].First(x => x.OwnerGuid == protocol.Guid);
            StopTasks(controller);
        }
        
        private void OnFull(AddResourceController controller)
        {
            if (!IsValid(controller)) return;
            OnResourceFull?.Invoke(controller.ItemID, controller.OwnerGuid);
        }

        private void OnChanged(AddResourceController controller)
        {
            if (!IsValid(controller)) return;
            OnResourceChanged?.Invoke(controller.ItemID, controller.OwnerGuid);
        }
    }
}