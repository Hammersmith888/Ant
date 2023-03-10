using System;
using UnityEngine;

namespace BugsFarm.TaskSystem
{
    public interface ITask
    {
        // Properties
        string Guid { get; }
        bool Interruptible { get; }
        // Properties
        
        // State
        bool IsRunned { get; }
        bool IsCompleted { get; }
        // State

        // Base
        event Action<ITask> OnExecute;
        event Action<ITask> OnComplete;
        event Action<ITask> OnForceComplete;
        event Action<ITask> OnInterrupt;

        void Execute(params object[] args);
        /// <summary>
        /// Закончить задачу моментально
        /// </summary>
        void ForceComplete();
        /// <summary>
        /// Прервать задачу
        /// </summary>
        void Interrupt();
        // Base
        // Model
        TaskParams GetRequirements();
        TaskParams GetRewards();
        Vector2[] GetPositions();
        string GetName();
        // Model
    }
}