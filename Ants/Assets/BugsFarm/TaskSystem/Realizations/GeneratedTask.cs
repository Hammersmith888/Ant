using System.Collections.Generic;
using System.Linq;
using BugsFarm.SimulationSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using UnityEngine.Profiling;

namespace BugsFarm.TaskSystem
{
    public class GeneratedTask : BootstrapTask
    {
        public override bool Interruptible { get; }
        private readonly TaskDtoStorage _taskDtoStorage;
        private readonly TaskDto _taskDto;
        private readonly List<object[]> _args;
        private readonly string _mainTaskName;
        
        public GeneratedTask(TaskDto taskDto,
                             bool interruptible,
                             IEnumerable<ITask> tasks,
                             IEnumerable<object[]> args,
                             ITickableManager tickableManager,
                             TaskDtoStorage taskDtoStorage) : base(tasks, tickableManager)
        {
            _args = args.ToList();
            _mainTaskName = Tasks.Last().GetName();
            Interruptible = interruptible;
            _taskDto = taskDto;
            _taskDtoStorage = taskDtoStorage;
        }

        public override string GetName()
        {
            return _mainTaskName;
        }
        
        protected override void SwitchTask()
        {
            if (CurrentTask != null) return;
            while (true) // recursion loop
            {
                if (Tasks.Count > 0)
                {
                    var task = Tasks[0];
                    Tasks.RemoveAt(0);

                    if (task.IsNullOrDefault())
                    {
                        continue;
                    }

                    var args = _args[0];
                    _args.RemoveAt(0);
                    task.Execute(args);
                    // задача может закончится моментально но,
                    // так как эта задача принимает группу полноценных задач,
                    // нужна пауза между группами задач.
                    // Этот коментарий к удалению этого кода :
                    // if (task.IsCompleted)
                    // {
                    //     continue;
                    // }
                    
                    CurrentTask = task;
                    return;
                }

                CurrentTask = null;
                Completed();
                break;
            }
        }

        protected override void OnDisposed()
        {
            if (!_taskDtoStorage.HasEntity(_taskDto.UnitGuid))
            {
                return;
            }
            var targetDto = _taskDtoStorage.Get(_taskDto.UnitGuid);

            if (targetDto == _taskDto)
            {
                _taskDtoStorage.Remove(targetDto.UnitGuid);
            }

            _args.Clear();
            base.OnDisposed();
        }
    }
}