using BugsFarm.AnimationsSystem;

namespace BugsFarm.UnitSystem
{
    public class UnitEatSystem : UnitNeedSystem
    {
        protected override string PrefixKey { get; } = "Eat_";
        protected override string ItemID { get; } = "0";                       // Предмет еды
        protected override AnimKey ConsumeAnimKey { get; } = AnimKey.Eat; // Анимация приема еды
        protected override string ResourceStatKey { get; } = "stat_consumeFood";
        protected override string NeedTimeStatKey { get; } = "stat_timeWithoutFood";
        protected override string NoNeedTimeStatKey { get; } = "stat_noNeedTimeFood";
    }
}