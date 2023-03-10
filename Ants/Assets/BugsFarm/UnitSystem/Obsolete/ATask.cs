using System;
using System.Runtime.Serialization;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum TaskStatus
    {
        NotAvailable,
        NotReached,

        InProcess,
        Completed,
    }

    public static class TaskUtils
    {
        /// <summary>
        /// Вернет true если задача не в работе
        /// </summary>
        public static bool IsCurrent(this TaskStatus status)
        {
            return status == TaskStatus.NotReached || status == TaskStatus.InProcess;
        }
    }

    [Serializable]
    public abstract class ATask
    {
        public delegate void ObjectEventHandler(APublisher publisher, ObjEvent objEvent);
        [NonSerialized] public ObjectEventHandler ObjectEvent;

        public Timer TimerTask { get; } = new Timer();
        public TaskStatus Status { get; private set; } = TaskStatus.NotAvailable;
        public bool IsLastCycle{ get; protected set; }
        public float TaskTime => _taskTime;
        private float _taskTime;
    
    
        //TODO : нужно избавитсья от не нужного флага WasOccupied
        public bool WasOccupied { get; protected set; }
    
        //TODO : нужно избавитсья от не нужного флага WasOccupied
        public abstract bool IsTaskEnd { get; }
    
        protected ATask()
        {
            SetObjectEventHandler();
        }
    
        [OnSerializing]
        internal virtual void OnSerializeMethod(StreamingContext ctx) { }
        [OnDeserialized]
        internal virtual void OnDeserializeMethod(StreamingContext ctx)
        {
            SetObjectEventHandler();
        }
    
        public void SetStatus(TaskStatus status)
        {
            Status = status;
        }
    
        public virtual bool TaskStart() { return true; }
        public void SetTaskTime(float taskTime)
        {
            _taskTime = taskTime;
        }
    
        public virtual void TaskExit(){}
        public virtual void Update(){}
        public virtual void ForceHardFinish(){}
    
        private void SetObjectEventHandler()
        {
            ObjectEvent = HandleObjectEvent;
        }
        protected void SetTaskTimer()
        {
            TimerTask.Set(_taskTime);
        }
        protected virtual void HandleObjectEvent(APublisher publisher, ObjEvent objEvent) { }
    }
}