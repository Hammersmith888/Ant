using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.InventorySystem;
using BugsFarm.TaskSystem;
using UnityEngine;
using UnityEngine.Profiling;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class TaskBuilderSystem
    {
        private readonly TaskStorage _taskStorage;
        private readonly IInstantiator _instantiator;
        private readonly TaskDtoStorage _taskDtoStorage;
        private readonly InventoryStorage _inventoryStorage;
        private readonly UnitTaskProcessorStorage _taskProcessorsStorage;
        public TaskBuilderSystem(IInstantiator instantiator,
                                 TaskStorage taskStorage, 
                                 TaskDtoStorage taskDtoStorage,
                                 InventoryStorage inventoryStorage,
                                 UnitTaskProcessorStorage taskProcessorsStorage)
        {
            _taskStorage = taskStorage;
            _instantiator = instantiator;
            _taskDtoStorage = taskDtoStorage;
            _inventoryStorage = inventoryStorage;
            _taskProcessorsStorage = taskProcessorsStorage;
        }

        public bool Build(TaskInfo taskInfo, string unitGuid, out ITask buildedTask)
        {
            buildedTask = default;
            
            var args = new List<object[]>();
            if (!_taskStorage.HasTask(taskInfo))
            {
                return false;
            }
            var task = _taskStorage.Get(taskInfo.TaskGuid);
            var sequence = new List<ITask>();
            if (!BuildInternal(unitGuid, task, sequence, args))
            {
                return false;
            }
            sequence.Reverse(); // выполнение в обратном порядке
            args.Reverse();
            
            foreach (var sequenceTask in sequence)
            {
                _taskStorage.Remove(sequenceTask.Guid);
            }
            
            var taskDto = new TaskDto
            {
                UnitGuid = unitGuid,
                TaskHash = taskInfo.TaskHash,
            };
            
            if (_taskDtoStorage.HasEntity(unitGuid))
            {
                _taskDtoStorage.Remove(unitGuid);
            }
            
            _taskDtoStorage.Add(taskDto);
            
            buildedTask = _instantiator.Instantiate<GeneratedTask>(new object[]{task.Interruptible, sequence, args, taskDto});
            return true;
        }        
        public bool Build(ITask task, string unitGuid, out ITask buildedTask)
        {
            buildedTask = default;
            
            var args = new List<object[]>();
            var sequence = new List<ITask>();
            if (!BuildInternal(unitGuid, task, sequence, args))
            {
                return false;
            }

            sequence.Reverse(); // выполнение в обратном порядке
            args.Reverse();
            
            foreach (var sequenceTask in sequence)
            {
                _taskStorage.Remove(sequenceTask.Guid);
            }
            
            var taskDto = new TaskDto
            {
                UnitGuid = unitGuid,
                TaskHash = unitGuid + task.GetRequirements().GetCustomHashCode() + task.GetRewards().GetCustomHashCode(),
            };
            
            if (_taskDtoStorage.HasEntity(unitGuid))
            {
                _taskDtoStorage.Remove(unitGuid);
            }
            
            _taskDtoStorage.Add(taskDto);
            
            buildedTask = _instantiator.Instantiate<GeneratedTask>(new object[]{task.Interruptible, sequence, args, taskDto});
            return true;
        }
        public bool Build(string unitGuid, out ITask buildedTask)
        {
            buildedTask = default;
            var args = new List<object[]>();
            if (!_taskDtoStorage.HasEntity(unitGuid))
            {
                return false;
            }

            var taskDto = _taskDtoStorage.Get(unitGuid);
            var findRequest = new FindRequest{WithHash = taskDto.TaskHash};
            var findedTasks = _taskStorage.FindTasks(findRequest).ToArray();
            
            if (findedTasks.Length == 0)
            {
                _taskDtoStorage.Remove(taskDto.UnitGuid);
                return false;
            }

            var task = findedTasks[0];
            var sequence = new List<ITask>();
            if (!BuildInternal(taskDto.UnitGuid, task, sequence, args))
            {
                _taskDtoStorage.Remove(taskDto.UnitGuid);
                return false;
            }
            
            sequence.Reverse(); // выполнение в обратном порядке
            args.Reverse();

            if (sequence.Count == 0)
            {
                _taskDtoStorage.Remove(taskDto.UnitGuid);
                return false;
            }


            foreach (var sequenceTask in sequence)
            {
                _taskStorage.Remove(sequenceTask.Guid);
            }
            
            buildedTask = _instantiator.Instantiate<GeneratedTask>(new object[]{task.Interruptible, sequence,taskDto, args});
            return true;
        }

        private bool BuildInternal(string unitGuid, 
                                   ITask task, 
                                   ICollection<ITask> sequence, 
                                   ICollection<object[]> args)
        {
            var requirements = task.GetRequirements();
            var givesReward  = task.GetRewards();
            var inventory = _inventoryStorage.Get(unitGuid);
            var ammountCarry = inventory.GetDefaultCapacity();
            ammountCarry = ammountCarry < 0 ? int.MaxValue : ammountCarry;
            // Важно : нулевой аргумент всегда для каждой задачи должен быть Guid юнита.
            // Важно : аргументы вознаграждения будут всегда идти перед аргументами требований
            // пример аргументов :
            // ( (string)UnitGuid, (int) GetCount..., (int) AddCount..., other args )
            var buildArgs = new List<object> {unitGuid};
            if (givesReward.Params.Length > 0)
            {
                // если предыдущей задаче нужно требование с определенным кол-вом предметов
                var needItems = new Dictionary<string, TaskParamModel>();
                if (sequence.Count > 0)
                {
                    needItems = sequence.Last()
                        .GetRequirements().Params
                        .Where(x => x.ID == TaskParamID.ItemID)
                        .ToDictionary(x=>x.Key);
                }

                foreach (var param in givesReward.Params)
                {
                    // получаем максимальное кол-во вознаграждения за текущую задачу
                    if (param.ID != TaskParamID.ItemID || !float.TryParse(param.Value, out var maxRewardValue)) continue;
                    // определяем кол-во для требования предыдущей задачи
                    if (needItems.ContainsKey(param.Key) && float.TryParse(needItems[param.Key].Value, out var needValue))
                    {
                        // Берем доступный минимум из :
                        // Юнит может нести,
                        // Нужно предыдущей задаче,
                        // Максимальное вознаграждение.
                        buildArgs.Add(Mathf.RoundToInt(Mathf.Min(ammountCarry, needValue, maxRewardValue)));
                        continue;
                    }
                    // если требований нет, аргумент будет равен 0 предметов для получения вознагражения.
                    buildArgs.Add(0);
                }
            }
            // Если есть требования, создаем запрос на поиск задачи
            if (requirements.Params.Length > 0)
            {
                var findRequest = new FindRequest();
                var withGivesReward = new List<string>();
                var skipRequirements = true;

                foreach (var param in requirements.Params)
                {
                    switch (param.ID)
                    {
                        case TaskParamID.WithoutModelID:
                            findRequest.WithoutModelIDs = Merge(findRequest.WithoutModelIDs,param.Value);
                            break;

                        case TaskParamID.WithoutGuid:
                            findRequest.WithoutGuids = Merge(findRequest.WithoutGuids,param.Value);
                            break;

                        case TaskParamID.WithModelID:
                            findRequest.WithModelIDs = Merge(findRequest.WithModelIDs,param.Value);
                            break;
                        
                        case TaskParamID.ItemID: 
                            // если в требованиях указываются предметы для получения
                            // создаем аргументы для задачи нужно кол-во предметов 
                            // добавляем предметы в запрос поиска.
                            if (!float.TryParse(param.Value, out var value))
                            {
                                throw new InvalidOperationException($"Requirements of task : {task.GetName()}. Param cannot Parse to float [ID : {param.ID}, Key : {param.Key}, Value : {param.Value}]");
                            }
                            // если у юнита есть в наличии предметы подходящие требованию
                            // если типов предметов не хватает,
                            // система будет искать задачи без существующего предмета.
                            if (inventory.HasItem(param.Key))
                            {
                                buildArgs.Add(inventory.GetItemSlot(param.Key).Count);
                                break;
                            }
                            skipRequirements = false;
                            withGivesReward.Add(param.Key);
                            buildArgs.Add(Mathf.RoundToInt(Mathf.Min(ammountCarry, value)));
                            break;
                    }
                }

                if (!skipRequirements) // пропускается если юнит имеет все нужные предметы
                {
                    
                    findRequest.WithGivesReward = withGivesReward.ToArray();
                    var findedTasks = _taskStorage.FindTasks(findRequest).ToList();
                    if (findedTasks.Count == 0)
                    {
                        return false;
                    }

                    Tools.Shuffle_FisherYates(findedTasks);
                    var unitTaskProcessor = _taskProcessorsStorage.Get(unitGuid);

                    foreach (var targetTask in findedTasks)
                    {
                        if (!unitTaskProcessor.CanExecute(targetTask))
                        {
                            continue;
                        }

                        sequence.Add(task);
                        args.Add(buildArgs.ToArray());
                        return BuildInternal(unitGuid, targetTask, sequence, args);
                    }

                    // требования не выполнимы
                    return false;
                }
            }
            
            // Требований к задачи нет, это последняя задача в последовательности
            sequence.Add(task);
            args.Add(buildArgs.ToArray());
            return true;
        }
        
        private T[] Merge<T>(T[] array, T obj)
        {
            if (array == null)
            {
                return new []{obj};
            }
            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = obj;
            return newArray;
        }
    }
}