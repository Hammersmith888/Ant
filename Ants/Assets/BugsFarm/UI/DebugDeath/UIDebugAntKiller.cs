using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class UIDebugAntKiller
    {
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;

        public UIDebugAntKiller(UnitSceneObjectStorage unitSceneObjectStorage, BuildingSceneObjectStorage buildingSceneObjectStorage, IUIService service)
        {
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _uiService = service;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }

        public void Initialize()
        {
            _uiService.Show<UIDebugDeathWindow>().OnButtonClicked += KillAnts;
        }

        private void KillAnts()
        {
            foreach (var unit in _unitSceneObjectStorage.Get())
            {
                MessageBroker.Default.Publish(new DeathUnitProtocol()
                {
                    UnitId = unit.Id,
                    DeathReason = DeathReason.Water
                });
            }

            foreach (var building in _buildingSceneObjectStorage.Get())
            {
                MessageBroker.Default.Publish(new DeathBuildingProtocol()
                {
                    Guid = building.Id
                });
            }
        }

        public void Dispose()
        {
            _uiService.Hide<UIDebugDeathWindow>();
        }
    }
}