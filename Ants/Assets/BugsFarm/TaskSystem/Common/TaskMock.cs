using System.Collections.Generic;
using System.Linq;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;
using UnityEngine;

namespace BugsFarm.TaskSystem
{
    /// <summary>
    /// С помощью этого класса, можно подменять проверки валидации.
    /// Этот класс можно создавать и изменять,чтобы не генерировать мусор -
    /// реальными задачами для простых валидаций.
    /// </summary>
    public class TaskMock : BaseTask
    {
        public override bool Interruptible { get; }
        private readonly string _taskName;
        private IEnumerable<IPosSide> _taskPoints;

        public TaskMock(string taskName, bool interruptible)
        {
            _taskName = taskName;
            Interruptible = interruptible;
            Finalized = true;
        }
        
        public override void Execute(params object[] args) { }
        
        public override string GetName()
        {
            return _taskName;
        }

        public void SetRequirements(TaskParams req)
        {
            Requirements = req;
        }
        
        public void SetRewards(TaskParams rewards)
        {
            GivesReward = rewards;
        }

        public void SetTaskPoints(params IPosSide[] taskPoints)
        {
            _taskPoints = taskPoints;
        }
        
        public void SetTaskPoints(params INode[] taskPoints)
        {
            _taskPoints = taskPoints.Select(x=> (IPosSide)new SPosSide(x.Position));
        }
        
        public override Vector2[] GetPositions()
        {
            return _taskPoints?.ToPositions() ?? base.GetPositions();
        }
    }
}