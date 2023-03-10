using System;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.StateMachine;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class DealerInit : State
    {
        private readonly IStateMachine _dealerStateMachine;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly TasksPoints _dealerFromPoints;
        private const string _dealerModelId = "10";
        private UnitDto _unitDto;

        
        public DealerInit(IStateMachine dealerStateMachine,
                          UnitDtoStorage unitDtoStorage,
                          PrefabLoader prefabLoader,
                          IInstantiator instantiator) : base("Init")
        {
            _dealerStateMachine = dealerStateMachine;
            _unitDtoStorage = unitDtoStorage;
            _instantiator = instantiator;
            var dealerPointsPrefab = prefabLoader.Load(nameof(Dealer) + "Points");
            _dealerFromPoints = _instantiator.InstantiatePrefabForComponent<TasksPoints>(dealerPointsPrefab);
        }

        public override void OnEnter(params object[] args)
        {
            if (_unitDto == null)
            {
                if (_unitDtoStorage.Get().Any(x=>x.ModelID == _dealerModelId))
                {
                    _unitDto = _unitDtoStorage.Get().First(x => x.ModelID == _dealerModelId);
                }
                else
                {
                    var createUnitProtocol = new CreateUnitProtocol(_dealerModelId, true);
                    _instantiator.Instantiate<CreateUnitCommand>().Execute(createUnitProtocol);

                    _instantiator
                        .Instantiate<SpawnUnitTask>(new object[] {_dealerFromPoints.Points.ElementAt(0).Position})
                        .Execute(createUnitProtocol.Guid);
                    _unitDto = _unitDtoStorage.Get(createUnitProtocol.Guid); 
                }
            }

            if (_unitDto == null)
            {
                throw new InvalidOperationException($"Unit : {nameof(Dealer)} does not initialized");
            }
            _dealerStateMachine.Switch("Process", _dealerFromPoints, _unitDto.Guid);
        }

        public override void OnExit()
        {
            _unitDto = null;
        }
    }
}