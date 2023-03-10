using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class SimulatingTaskAssigner
    {
        private Dictionary<string, ITaskAssigner> _assigners;

        public SimulatingTaskAssigner(IInstantiator instantiator)
        {
            _assigners = new Dictionary<string, ITaskAssigner>()
            {
               //{"Eat", instantiator.Instantiate<EatTaskAssigner>()},
                //{"Drink", instantiator.Instantiate<DrinkTaskAssigner>()},
                {"Goldmine", instantiator.Instantiate<GoldmineTaskAssigner>()},
                {"Train", instantiator.Instantiate<TrainTaskAssigner>()},
                {"Dig", instantiator.Instantiate<DigTaskAssigner>()},
                {"Patrol", instantiator.Instantiate<PatrolTaskAssigner>()},
                {"Repair", instantiator.Instantiate<RepairTaskAssigner>()},
                {"GardenCare", instantiator.Instantiate<GardenCareAssignTask>()},
            };
        }

        public void ProcessTasks(string guid, IEnumerable<SimulatingAssignableTask> tasks)
        {
            foreach (var task in tasks)
            {
                if (_assigners.ContainsKey(task.TaskName) && _assigners[task.TaskName].CanAssign(guid))
                {
                    _assigners[task.TaskName].Assign(guid);
                    return;
                }


            }
        }
    }
}