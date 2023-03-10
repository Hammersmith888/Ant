using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class BirthLarvaSimulatingStage
    {
        private readonly OpenRoomsSimulatingStage _openRoomsSimulatingStage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitBirthModelStorage _unitBirthModelStorage;
        private readonly SimulatingAntLarvaCreator _antLarvaCreator;
        private readonly UnitDtoStorage _unitDtoStorage;

        private const string _betweenBornTimeStatKey = "stat_betwenBornTime";
        private const string _maxMaggotsStatKey = "stat_maxMaggots";
        private const string _bornPerRoomStatKey = "stat_bornPerRoom";
        private const string _queenTypeName = "Queen";
        private const float _totalLarvaGrowTime = 1440.0f;

        private StatsCollection _statCollection;
        private float _leftToBorn;
        private readonly IUser _iuser;

        public BirthLarvaSimulatingStage(StatsCollectionStorage statsCollectionStorage,
            SimulatingAntLarvaCreator antLarvaCreator,
            OpenRoomsSimulatingStage openRoomsSimulatingStage, IUser user,
            UnitDtoStorage unitDtoStorage,
            UnitBirthModelStorage unitBirthModelStorage)
        {
            _openRoomsSimulatingStage = openRoomsSimulatingStage;
            _statsCollectionStorage = statsCollectionStorage;
            _unitBirthModelStorage = unitBirthModelStorage;
            _antLarvaCreator = antLarvaCreator;
            _unitDtoStorage = unitDtoStorage;
            _iuser = user;

        }

        public void BirthLarvas(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulationData, float cycleNum)
        {
            if (!simulationData.ContainsKey(SimulatingEntityType.Queen))
                return;

            RegisterExisting(simulationData, minutesInCycle, cycleNum);
            
            _statCollection = _statsCollectionStorage.Get(simulationData[SimulatingEntityType.Queen][0].Guid);
            StatVital bornInterval = _statCollection.Get<StatVital>(_betweenBornTimeStatKey);

            if (minutesInCycle + _leftToBorn > bornInterval.CurrentValue)
            {
                _leftToBorn = 0.0f;
                BirthNew(minutesInCycle, cycleNum);
            }
            else
            {
                _leftToBorn += (bornInterval.CurrentValue - minutesInCycle);
            }

        }

        private void RegisterExisting(Dictionary<string, List<SimulatingEntityDto>> simulationData, float minutesInCycle, float cycleNum)
        {
            if (!simulationData.ContainsKey(SimulatingEntityType.Larva))
                return;
            var birthData = _unitBirthModelStorage.Get(_queenTypeName);
            for (int i = 0; i < simulationData[SimulatingEntityType.Larva].Count; i++)
            {
                var data = simulationData[SimulatingEntityType.Larva][i];
                if (!_antLarvaCreator.IsRegistered(data.Guid))
                {
                    var randomUnit = birthData.BirthUnitsModelID[UnityEngine.Random.Range(0, birthData.BirthUnitsModelID.Length)];
                    _antLarvaCreator.RegisterNewLarva(data.Guid, birthData.LarvaModelID, randomUnit, _totalLarvaGrowTime, false, minutesInCycle * cycleNum);
                }
            }
        }

        private int LarvaToBirth()
        {
            var rooms = _openRoomsSimulatingStage.OpenedRoomsAmount;
            var units = _unitDtoStorage.Get().ToArray();

            float count = units.Count(x => _unitBirthModelStorage.Get(_queenTypeName).TrackingUnitsModelID.Contains(x.ModelID));
            return Mathf.RoundToInt(Mathf.Max(0, _statCollection.GetValue(_bornPerRoomStatKey) - (count / rooms)));
        }

        private void BirthNew(float minutesInCycle, float cycleNum)
        {
            var unitCount = LarvaToBirth();
            if (unitCount <= 0)
                return;
            var birthData = _unitBirthModelStorage.Get(_queenTypeName);
            for (int i = 0; i < unitCount; i++)
            {
                if (!_antLarvaCreator.HasFreePlace())
                    return;
                var randomUnit = birthData.BirthUnitsModelID[UnityEngine.Random.Range(0, birthData.BirthUnitsModelID.Length)];
                _antLarvaCreator.RegisterNewLarva(Guid.NewGuid().ToString(),birthData.LarvaModelID, randomUnit, _totalLarvaGrowTime, true, minutesInCycle * cycleNum);
            }
        }
    }
}