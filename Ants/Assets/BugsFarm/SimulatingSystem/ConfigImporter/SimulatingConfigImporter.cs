using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BugsFarm.Services.SheetLoader;
using BugsFarm.Utility;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingConfigImporter : SheetLoader
    {
        [SerializeField] private bool _prettyPrint;
        
        [ExposeMethodInEditor]
        private void LoadSimulationBuildingGroups()
        {
            Load("BuildingsSimulationGroups", data =>
            {
                var config = new List<BuildingsSimulationGroup>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new BuildingsSimulationGroup()
                    {
                        ModelID = entries[1],
                        SimulationGroup = entries[2]
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "BuildingsSimulationGroups", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void LoadSimulationFoodStorageOrder()
        {
            Load("SimulationFoodOrder", data =>
            {
                var config = new List<SimulatingFoodOrderModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new SimulatingFoodOrderModel()
                    {
                        ModelID = entries[1],
                        Priority = int.Parse(entries[2])
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "SimulatingFoodOrderModels", _prettyPrint);
            });
        }
        [ExposeMethodInEditor]
        private void LoadSimulationUnitGroups()
        {
            Load("UnitsSimulationGroups", data =>
            {
                var config = new List<UnitsSimulationGroupModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new UnitsSimulationGroupModel()
                    {
                        ModelID = entries[1],
                        SimulationGroup = entries[2]
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "UnitSimulationGroupModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void LoadSimulationRoomGroups()
        {
            Load("SimulatingRoomGroups", data =>
            {
                var config = new List<SimulatingRoomGroupModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new SimulatingRoomGroupModel()
                    {
                        TypeName = entries[0],
                        Group = entries[1]
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "SimulatingRoomGroupModels", _prettyPrint);
            });
        }

        [ExposeMethodInEditor]
        private void LoadSimulationTrainingData()
        {
            Load("SimulationTraining", data =>
            {
                var config = new List<SimulatingTrainingModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new SimulatingTrainingModel()
                    {
                        ModelID = entries[1],
                        BuildingsModelID = entries[2].Split('/')
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "SimulatingTrainingModels", _prettyPrint);
            });
        }
        
        [ExposeMethodInEditor]
        private void LoadSimulationUnitSleep()
        {
            Load("SimulatingSleepDuration", data =>
            {
                var config = new List<SimulatingUnitSleepModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
                    
                    var roomModel = new SimulatingUnitSleepModel()
                    {
                        ModelID = entries[1],
                        Duration = float.Parse(entries[2], CultureInfo.InvariantCulture)
                    };
                    config.Add(roomModel);
                }

                ConfigHelper.Save(config.ToArray(), "SimulatingUnitSleepModels", _prettyPrint);
            });
        }

        
        [ExposeMethodInEditor]
        private void LoadAssignableTaskData()
        {
            Load("SimulatingAssignableTasks", data =>
            {
                var config = new List<SimulatingUnitAssignableTaskModel>();
                var lines = data.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
                lines.RemoveAt(0); // headers
                var modelID = "0";
                var tasks = new List<SimulatingAssignableTask>();
                foreach (var line in lines)
                {
                    var entries = line.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);

                    if (modelID != entries[1])
                    {
                        var assignableTaskModel = new SimulatingUnitAssignableTaskModel()
                        {
                            ModelID = modelID,
                            AssignableTasks = tasks.ToArray()
                        };
                        config.Add(assignableTaskModel);
                        tasks.Clear();
                        modelID = entries[1];
                    }

                    var task = new SimulatingAssignableTask()
                    {
                        TaskName = entries[2],
                        Time = float.Parse(entries[3], CultureInfo.InvariantCulture)
                    };
                    tasks.Add(task);
                }
                var taskModel = new SimulatingUnitAssignableTaskModel()
                {
                    ModelID = modelID,
                    AssignableTasks = tasks.ToArray()
                };
                config.Add(taskModel);
                tasks.Clear();
                ConfigHelper.Save(config.ToArray(), "SimulatingUnitAssignableTaskModels", _prettyPrint);
            });
        }
    }
}