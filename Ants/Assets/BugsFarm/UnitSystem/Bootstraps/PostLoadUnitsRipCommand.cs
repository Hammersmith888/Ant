using BugsFarm.Services.BootstrapService;

namespace BugsFarm.UnitSystem
{
    public class PostLoadUnitsRipCommand : Command
    {
        private readonly UnitCivilRegistrySystem _unitCivilRegistrySystem;
        private readonly UnitCivilRegistryDtoStorage _unitCivilRegistryDtoStorage;

        public PostLoadUnitsRipCommand(UnitCivilRegistrySystem unitCivilRegistrySystem,
                                   UnitCivilRegistryDtoStorage unitCivilRegistryDtoStorage)
        {
            _unitCivilRegistrySystem = unitCivilRegistrySystem;
            _unitCivilRegistryDtoStorage = unitCivilRegistryDtoStorage;
        }
        public override void Do()
        {
            foreach (var registry in _unitCivilRegistryDtoStorage.Get())
            {
                _unitCivilRegistrySystem.SpawnRip(registry, true);
            }
            OnDone();
        }
    }
}