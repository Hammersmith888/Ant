using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;
using BugsFarm.ReloadSystem;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitSleepSystem : IDisposable
    {
        public event Action<string> OnSleepy;
        private readonly IInstantiator _instantiator;
        private readonly UnitTaskProcessorStorage _taskProcessorsStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly PathHelper _pathHelper;
        private readonly Dictionary<string, NeedStatController> _storage;
        private readonly List<string> _sleepy;
        private readonly List<string> _sleep;

        private const string _prefixKey = "Sleep_";
        private const string _resourceStatKey   = "stat_consumeSleep";
        private const string _needTimeStatKey   = "stat_timeWithoutSleep";
        private const string _noNeedTimeStatKey = "stat_noNeedTimeSleep";
        private readonly TaskMock _sleepTaskMock;
        public UnitSleepSystem(IInstantiator instantiator,
                               UnitTaskProcessorStorage taskProcessorsStorage,
                               UnitMoverStorage unitMoverStorage,
                               UnitDtoStorage unitDtoStorage,
                               PathHelper pathHelper)
        {
            _instantiator = instantiator;
            _taskProcessorsStorage = taskProcessorsStorage;
            _unitMoverStorage = unitMoverStorage;
            _unitDtoStorage = unitDtoStorage;
            _pathHelper = pathHelper;
            _storage = new Dictionary<string, NeedStatController>();
            _sleepy = new List<string>();
            _sleep = new List<string>();
            _sleepTaskMock = _instantiator.Instantiate<TaskMock>(new object[] {nameof(SleepUnitBootstrapTask), true});
        }

        public void Registration(string guid)
        {
            if (HasUnit(guid))
            {
                throw new
                    InvalidOperationException($"{this} : Registration :: Unit with [Guid : {guid}], alredy exist.");
            }

            var controllerProtocol = new NeedStatPtotocol(guid, _prefixKey, _resourceStatKey, 
                                                          _noNeedTimeStatKey, _needTimeStatKey, OnNeedRestock);
            var controller = _instantiator.Instantiate<NeedStatController>(new object[] {controllerProtocol});
            _storage.Add(guid, controller);
        }

        public void UnRegistration(string guid)
        {
            if (HasUnit(guid))
            {
                var controller = _storage[guid];
                controller.Dispose();
                _storage.Remove(guid);
            }
            
            if (IsSleep(guid) && _taskProcessorsStorage.HasEntity(guid))
            {
                var taskProcessor = _taskProcessorsStorage.Get(guid);
                taskProcessor.Interrupt();
                _sleep.Remove(guid);
            }

            if (IsSleepy(guid))
            {
                _sleepy.Remove(guid);
            }
        }

        public void Dispose()
        {
            OnSleepy = null;
            var units = _storage.Keys.ToArray();
            if (!GameReloader.IsReloading)
            {
                foreach (var unit in units)
                {
                    UnRegistration(unit);
                }    
            }

            _storage.Clear();
            _sleep.Clear();
            _sleepy.Clear();
        }

        public bool HasUnit(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public bool IsSleepy(string guid)
        {
            return _sleepy.Contains(guid);
        }

        public bool IsSleep(string guid)
        {
            return _sleep.Contains(guid);
        }

        public void Awake(string guid)
        {
            if (!HasUnit(guid))
                return;

            if (!IsSleep(guid) || !_taskProcessorsStorage.HasEntity(guid))
                return;

            var taskProcessor = _taskProcessorsStorage.Get(guid);
            taskProcessor.Interrupt();
        }

        public NeedStatController GetController(string guid)
        {
            if (!HasUnit(guid))
            {
                return null;
            }

            return _storage[guid];
        }
        
        public void AwakeAll()
        {
            var temp = _sleep.ToArray();
            foreach (var guid in temp)
            {
                Awake(guid);
            }
        }

        public INeedInfo GetInfo(string guid)
        {
            return HasUnit(guid) ? _storage[guid] : default;
        }

        public bool Start(string unitId, INode position = null)
        {
            if (!HasUnit(unitId)) return false;
            if (IsSleep(unitId)) return false;

            if (!IsSleepy(unitId)) return false;

            var taskProcessor = _taskProcessorsStorage.Get(unitId);
            var mover = _unitMoverStorage.Get(unitId);
            var unitDto = _unitDtoStorage.Get(unitId);
            Vector2 taskPoint;
            INode nodePos;

            if (position == null)
            {
                var pathHelperQuery = PathHelperQuery.Empty()
                    .UseGraphMask(GetType().Name)
                    .UseLimitationsCheck(unitDto.ModelID)
                    .UseTraversableCheck(mover.TraversableTags);
                var posSide = _pathHelper.GetRandomNodes(pathHelperQuery).First();
                nodePos = posSide;
                taskPoint = posSide.Position;
            }
            else
            {
                taskPoint = position.Position;
                nodePos = position;
            }

 
            _sleepTaskMock.SetTaskPoints(nodePos);
            if (!taskProcessor.CanInterrupt(_sleepTaskMock)) return false;
            var controller = _storage[unitId];
            var args = new object[] {controller, taskPoint};
            var sleepTask = _instantiator.Instantiate<SleepUnitBootstrapTask>(args);
            
            _sleepy.Remove(unitId);
            _sleep.Add(unitId);
            sleepTask.OnComplete  += _ => OnTaskEnd(unitId);
            sleepTask.OnInterrupt += _ => OnTaskEnd(unitId);
            taskProcessor.SetTask(sleepTask);
            return true;
        }

       /* public bool Start(string unitId, INode position)
        {
            if (!HasUnit(unitId)) return false;
            if (IsSleep(unitId)) return false;
            if (!IsSleepy(unitId)) return false;
            
            var taskProcessor = _taskProcessorsStorage.Get(unitId);
            _sleepTaskMock.SetTaskPoints(position);
            if (!taskProcessor.CanInterrupt(_sleepTaskMock)) return false;
            var controller = _storage[unitId];
            var sleepTask = _instantiator.Instantiate<SleepTask>(new object[] {controller});
            _sleepy.Remove(unitId);
            _sleep.Add(unitId);
            sleepTask.OnComplete  += _ => OnTaskEnd(unitId);
            sleepTask.OnInterrupt += _ => OnTaskEnd(unitId);
            taskProcessor.SetTask(sleepTask);
            return true;
        }*/
        
        public void SetMax(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            var controller = _storage[guid];
            controller.Update(controller.NeedCount);
            controller.UseAvailable();
            Awake(guid);
        }

        private void OnNeedRestock(string guid)
        {
            if (!HasUnit(guid)) return;

            if (!IsSleepy(guid))
            {
                _sleepy.Add(guid);
            }

            OnSleepy?.Invoke(guid);
        }

        private void OnTaskEnd(string guid)
        {
            if (!HasUnit(guid)) return;
            if (IsSleep(guid))
            {
                _sleep.Remove(guid);
            }

            if (IsSleepy(guid))
            {
                _sleepy.Remove(guid);
            }
        }

        public void AddToSleepy(string unitDtoGuid)
        {
            if (_sleepy.Contains(unitDtoGuid))
                return;
            _sleepy.Add(unitDtoGuid);
        }
    }
}