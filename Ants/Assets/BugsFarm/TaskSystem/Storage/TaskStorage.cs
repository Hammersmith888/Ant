using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.UnitSystem;

namespace BugsFarm.TaskSystem
{
    public class TaskStorage
    {
        public event Action<string> TaskRemoved;
        public event Action<TaskInfo> TaskDeclare;
        
        private readonly List<TaskInfo> _definedTaskInfos;
        /// <summary>
        /// arg1 = OwnerId , (arg1 = taskGuid, arg2 = task)
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, ITask>> _storage;
        public TaskStorage()
        {
            _definedTaskInfos = new List<TaskInfo>();
            _storage = new Dictionary<string, Dictionary<string, ITask>>();
        }
        
        public void DeclareTask(string ownerId, string ownerModelID, string taskName, ITask task, bool notify = true)
        {
            if (!_storage.ContainsKey(ownerId))
            {
                _storage.Add(ownerId, new Dictionary<string, ITask>());
            }

            _storage[ownerId].Add(task.Guid, task);
            var taskInfo = new TaskInfo(taskName, ownerId, ownerModelID, task);
            _definedTaskInfos.Add(taskInfo);
            if(notify)
            {
                TaskDeclare?.Invoke(taskInfo);
            }
        }
        
        public void RemoveAll(string ownerId)
        {
            var taskInfos = GetAllInfos(ownerId).ToArray();
            if (taskInfos.Length == 0)
            {
                return;
            }

            foreach (var taskInfo in taskInfos)
            {
                Remove(taskInfo);
            }
        }
        
        public void Remove(string taskGuid)
        {
            var taskInfo = GetTaskInfo(taskGuid);
            if (taskInfo == null)
            {
                return;
            }
            Remove(taskInfo);
        }
        
        public bool HasTask(TaskInfo taskInfo)
        {
            return _definedTaskInfos.Contains(taskInfo);
        }        
        
        public bool HasTask(string taskGuid)
        {
            return !GetTaskInfo(taskGuid).IsNullOrDefault();
        }
        
        public bool HasTasks()
        {
            return _definedTaskInfos.Any();
        }
        
        public bool HasTasks(string ownerId)
        {
            return _storage.ContainsKey(ownerId);
        }

        public IEnumerable<ITask> FindTasks(FindRequest request)
        {
            var listTasks = new List<ITask>();
            
            foreach (var taskInfo in GetAllInfo())
            {
                if(taskInfo.IsNullOrDefault()) continue;
                
                // находим задачу с идентичным хэш кодом
                if (request.WithHash == taskInfo.TaskHash)
                {
                    listTasks.Add(Get(taskInfo));
                    break;
                }
                // пропускаем все с требованиями
                if (request.WithoutRequirements && taskInfo.Requirements.Params.Length > 0) continue;
                
                // пропускаем все от ненужного гуида
                if(request.WithoutGuids.Length > 0 && request.WithoutGuids.Contains(taskInfo.OwnerId)) continue;
                
                // пропускаем все от ненужных моделей ид
                if (request.WithoutModelIDs.Length > 0 && request.WithoutModelIDs.Contains(taskInfo.OwnerModelID)) continue;
                
                // пропускаем все модели которые не нужны
                if (request.WithModelIDs.Length > 0 && !request.WithModelIDs.Contains(taskInfo.OwnerModelID)) continue;
                
                if (request.WithGivesReward.Length > 0)
                {
                    var requirements = request.WithGivesReward;
                    var rewardsTargetTask = taskInfo.GivesReward.Params.Select(x=>x.Key).ToArray();

                    if (rewardsTargetTask.Length == 0)
                    {
                        continue;
                    }

                    if (rewardsTargetTask.Length < requirements.Length)
                    {
                        continue;
                    }
                    
                    if (requirements.Any(requirement => !rewardsTargetTask.Contains(requirement)))
                    {
                        continue;
                    }

                    // если достаточно совпадений то выбираем задачу
                    listTasks.Add(Get(taskInfo));
                }
            }
            
            return listTasks.Where(x=>x != null);
        }

        public IEnumerable<TaskInfo> GetAllInfo()
        {
            return _definedTaskInfos;
        }
        
        public IEnumerable<TaskInfo> GetAllInfos(string sourceGuid)
        {
            return _definedTaskInfos.Where(x => x.OwnerId == sourceGuid);
        }
        
        /// <summary>
        /// Старайтесь не использовать получение задач на прямую,
        /// их могут забрать из хранилища и использовать
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITask> GetAll(string sourceGuid)
        {
            return _storage.TryGetValue(sourceGuid,out var tasks) ? tasks.Values : default;
        }
        
        /// <summary>
        /// Старайтесь не использовать получение задач на прямую,
        /// их могут забрать из хранилища и использовать
        /// </summary>
        /// <returns></returns>
        public ITask Get(string taskGuid)
        {
            var taskInfo = GetTaskInfo(taskGuid);
            return taskInfo.IsNullOrDefault() ? default : _storage[taskInfo.OwnerId][taskGuid];
        }
        
        /// <summary>
        /// Старайтесь не использовать получение задач на прямую,
        /// их могут забрать из хранилища и использовать
        /// </summary>
        /// <returns></returns>
        public ITask Get(TaskInfo taskInfo)
        {
            return taskInfo.IsNullOrDefault() ? default : _storage[taskInfo.OwnerId][taskInfo.TaskGuid];
        }
        
        /// <summary>
        /// Старайтесь не использовать получение задач на прямую,
        /// их могут забрать из хранилища и использовать
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ITask> Get()
        {
            return _storage.SelectMany(x=>x.Value.Values);
        }
        
        public TaskInfo GetTaskInfo(string taskGuid)
        {
            var copy = _definedTaskInfos.ToArray();
            return copy.FirstOrDefault(x => x.TaskGuid == taskGuid);
        }
        
        private void Remove(TaskInfo taskInfo)
        {
            if (!HasTask(taskInfo))
            {
                return;
            }

            _definedTaskInfos.Remove(taskInfo);
            _storage[taskInfo.OwnerId].Remove(taskInfo.TaskGuid);
            if (_storage[taskInfo.OwnerId].Count == 0)
            {
                _storage.Remove(taskInfo.OwnerId);
            }
            TaskRemoved?.Invoke(taskInfo.TaskGuid);
        }
    }
}