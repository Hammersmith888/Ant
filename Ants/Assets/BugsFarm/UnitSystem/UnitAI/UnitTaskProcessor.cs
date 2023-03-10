using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitTaskProcessor : IInitializable, IDisposable, IUnitTaskProcessor
    {
        public bool IsFree { get; private set; } = true;
        public string TaskName => _currentTask?.GetName() ?? string.Empty;
        public event Action<ITask> OnFree;

        private readonly TaskStorage _taskStorage;
        private readonly TaskBuilderSystem _taskBuilderSystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitTaskProcessorStorage _taskProcessorsStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitTaskModelStorage _taskModelsStorage;
        private readonly TaskDtoStorage _taskDtoStorage;


        /// <summary>
        /// arg1 = название задачи, arg2 = приоритет
        /// </summary>
        private Dictionary<string, int> _tasks;

        private ITask _currentTask;
        private bool _canUpdate;
        private bool _prepareTask;

        public string Id { get; }

        public UnitTaskProcessor(string guid,
                                 TaskStorage taskStorage,
                                 TaskBuilderSystem taskBuilderSystem,
                                 UnitDtoStorage unitDtoStorage,
                                 UnitTaskProcessorStorage taskProcessorsStorage,
                                 UnitMoverStorage unitMoverStorage,
                                 UnitTaskModelStorage taskModelsStorage,
                                 TaskDtoStorage taskDtoStorage)
        {
            Id = guid;
            _taskStorage = taskStorage;
            _taskBuilderSystem = taskBuilderSystem;
            _unitDtoStorage = unitDtoStorage;
            _taskProcessorsStorage = taskProcessorsStorage;
            _unitMoverStorage = unitMoverStorage;
            _taskModelsStorage = taskModelsStorage;
            _taskDtoStorage = taskDtoStorage;
        }

        #region Zenject

        public void Initialize()
        {
            var unitDto = _unitDtoStorage.Get(Id);
            _tasks = new Dictionary<string, int>();
            if (_taskModelsStorage.HasEntity(unitDto.ModelID))
            {
                var tasks = _taskModelsStorage.Get(unitDto.ModelID).Tasks;
                for (var i = 0; i < tasks.Length; i++)
                {
                    var task = tasks[i];
                    _tasks.Add(task, i);
                }
            }

            _taskProcessorsStorage.Add(this);
            _taskStorage.TaskDeclare += OnTaskDeclare;
        }

        public void Dispose()
        {
            if (_taskStorage != null)
            {
                _taskStorage.TaskDeclare -= OnTaskDeclare;
            }

            if(_taskProcessorsStorage.HasEntity(Id))
                _taskProcessorsStorage?.Remove(Id);

            _tasks = null;
            OnFree = null;
            _currentTask = null;
        }

        #endregion

        #region IUnitTaskProcessor

        public void SetTask(ITask task, params object[] args)
        {
            if (!IsExecutableTask(task.GetName()))
            {
                return;
                throw new ArgumentException("Task cannot be executed because task not registered in model.");
            }
            SetupTask(task, args);
        }

        public ITask GetCurrentTask()
        {
            return _currentTask;
        }
        
        public bool CanExecute(ITask task)
        {
            return task != null && CanReach(task);
        }

        public void Interrupt()
        {
            _currentTask?.Interrupt();
        }
        public bool CanInterrupt(ITask other = null)
        {
            if (_currentTask != null && !_currentTask.Interruptible)
            {
                return false;
            }

            return other == null || HigherPriority(other.GetName());
        }

        #endregion

        #region Task Process

        public void Stop()
        {
            if (!_canUpdate) return;

            _canUpdate = false;
        }

        public void Play()
        {
            if (_canUpdate) return;

            _canUpdate = true;
        }

        public bool TryStartInterrupted()
        {
            if (!_taskDtoStorage.HasEntity(Id)) return false;
            
            _prepareTask = true;
            if (_taskBuilderSystem.Build(Id, out var buildedTask))
            {
                _prepareTask = false;
                SetupTask(buildedTask);
                return true;
            }
            _prepareTask = false;
            return false;
        }
        
        public void Update()
        {
            if (_prepareTask || (!_taskStorage.HasTasks() && !_taskDtoStorage.HasEntity(Id)))  
                return;

            _prepareTask = true;
            var orderedTasks = _taskStorage.GetAllInfo()
                .Where(x=> IsExecutableTask(x.TaskName))
                .OrderBy(x=> _tasks[x.TaskName]);

            foreach (var taskInfo in orderedTasks)
            {
                if (!CanExecute(taskInfo) || !_taskBuilderSystem.Build(taskInfo, Id, out var buildedTask))
                {
                    continue;
                }

                _prepareTask = false;
                SetupTask(buildedTask);
                return;
            }
            _prepareTask = false;
        }

        private bool HigherPriority(string taskName)
        {
            return IsExecutableTask(taskName) && 
                (string.IsNullOrEmpty(TaskName) || 
                (_tasks[taskName] < _tasks[TaskName]));
        }

        private bool IsExecutableTask(string taskName)
        {
            return _tasks.ContainsKey(taskName);
        }

        private void OnTaskDeclare(TaskInfo taskInfo)
        {
            if (!_canUpdate || _prepareTask || !CanExecute(taskInfo)) {return;}

            _prepareTask = true;
            if (_taskBuilderSystem.Build(taskInfo, Id, out var buildedTask))
            {
                _prepareTask = false;
                SetupTask(buildedTask);
                return;
            }
            _prepareTask = false;
        }

        private void SetupTask(ITask task, params object[] args)
        {

            if (_prepareTask || task == null)
            {
                return;
            }

            _prepareTask = true;
            args = args.Prepend(Id).ToArray();

            Interrupt();
            _currentTask = task;
            _currentTask.OnComplete  += OnTaskEnd;
            _currentTask.OnInterrupt += OnTaskEnd;
            _currentTask.OnForceComplete += OnTaskEnd;
            _prepareTask = IsFree = false;
            _currentTask.Execute(args);
        }

        private void OnTaskEnd(ITask task)
        {
            if (IsFree || _prepareTask) return;
            _currentTask = null;
            IsFree = true;
            OnFree?.Invoke(task);
        }
        
        private bool CanExecute(TaskInfo taskInfo)
        {
            return taskInfo != null &&                              // проверка наличие экземпляра
                   CanInterrupt() &&                                // можно ли прирвать текущую задачу если она есть
                   HigherPriority(taskInfo.TaskName) &&             // сравниваем приоритеты задач
                   _taskStorage.HasTask(taskInfo) &&                // пост проверка не забрали задачу другие
                   CanReach(_taskStorage.Get(taskInfo.TaskGuid));   // проверка на досягаемость мест выполнения задач

        }
        
        private bool CanReach(ITask task)
        {
            if (task == null || task.IsRunned || task.IsCompleted)
            {
                return false;
            }

            if (!_unitMoverStorage.HasEntity(Id))
            {
                return false;
            }
            
            var mover = _unitMoverStorage.Get(Id);
            var taskPositions = task.GetPositions();
            return taskPositions.Length == 0 || mover.CanReachTarget(taskPositions);
        }

        #endregion
        
    }
}