using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Utility;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class PostLoadBuildingsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly BuildingDtoStorage _buildingDtoStorage;

        public PostLoadBuildingsCommand(IInstantiator instantiator, 
                                        BuildingDtoStorage buildingDtoStorage)
        {
            _instantiator = instantiator;
            _buildingDtoStorage = buildingDtoStorage;
        }
        public override void Do()
        {
            var postLoadPriorities = ConfigHelper.Load<string>("PostLoadBuildingPriorities").ToList();
            var dtos = _buildingDtoStorage.Get().ToArray();
            var sorted = new List<BuildingDto>[postLoadPriorities.Count];
            
            foreach (var buildingDto in dtos)
            {
                var targetIndex = postLoadPriorities.IndexOf(buildingDto.ModelID);
                if (targetIndex == -1)
                {
                    targetIndex = postLoadPriorities.Count - 1;
                    Debug.Log($"{this} : Building with ModelID : {buildingDto.ModelID} no priority " +
                              $"in PostLoadBuildingPriorities.json, lower priority set : {targetIndex}");
                }
                var targetList = sorted[targetIndex] ?? new List<BuildingDto>();
                targetList.Add(buildingDto);
                sorted[targetIndex] = targetList;
            }
            foreach (var priorityList in sorted)
            {
                if(priorityList == null) continue;
                
                foreach (var buildingDto in priorityList)
                {
                    _instantiator
                        .Instantiate<CreateBuildingCommand>()
                        .Execute(new CreateBuildingProtocol(buildingDto.Guid, buildingDto.PlaceNum, false, true));
                }
            }
            OnDone();
        }
    }
}