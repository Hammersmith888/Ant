using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.RoomSystem;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class RoomAntHillTaskProcessor : BaseAntHillTaskProcessor
    {
        private Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>> _conditions;
        private readonly IRoomsSystem _roomsSystem;
        private readonly RoomDtoStorage _roomDtoStorage;

        public RoomAntHillTaskProcessor(IRoomsSystem roomsSystem, RoomDtoStorage roomDtoStorage)
        {
            _roomDtoStorage = roomDtoStorage;
            _roomsSystem = roomsSystem;
            _conditions = new Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>>()
            {
                {AntHillTaskCondition.None, (x, y) => true},
                {AntHillTaskCondition.Amount, HasAmountOf}
            };
        }

        public override void RefreshAmount(AntHillTaskDto taskDto, bool add = false)
        {
            switch (taskDto.ProgressWay)
            {
                case "Amount":
                    taskDto.CurrentValue = GetAmount(taskDto.ReferenceModelID);
                    break;
            }
        }

        private int GetAmount(string[] referenceModelID)
        {
            var opened = _roomsSystem.Opened();
            return opened.Count(x => referenceModelID.Contains(_roomDtoStorage.Get(x).ModelID));
        }
        private bool HasAmountOf(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            return _roomsSystem.Opened().Count() >= taskDto.ConditionValue;
        }
        protected override bool IsConditionCompleted(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            return _conditions[taskDto.ConditionName](protocol, taskDto);
        }
    }
}