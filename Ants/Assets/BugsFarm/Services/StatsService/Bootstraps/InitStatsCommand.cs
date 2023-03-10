using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.TypeRegistry;

namespace BugsFarm.Services.StatsService
{
    public class InitStatsCommand : Command
    {
        private readonly TypeStorage _typeStorage;

        public InitStatsCommand(TypeStorage typeStorage)
        {
            _typeStorage = typeStorage;
        }
        public override void Do()
        {
            _typeStorage.Registry(nameof(Stat), 
                                  nameof(StatModifiable), 
                                  nameof(StatAttribute), 
                                  nameof(StatVital),
                                  nameof(StatDto),
                                  nameof(StatModifiableDto),
                                  nameof(StatAttributeDto),
                                  nameof(StatVitalDto),
                                  nameof(StatModBaseAdd),
                                  nameof(StatModBasePercent), 
                                  nameof(StatModTotalAdd), 
                                  nameof(StatModTotalPercent),
                                  nameof(StatLinker));
            OnDone();
        }
    }
}