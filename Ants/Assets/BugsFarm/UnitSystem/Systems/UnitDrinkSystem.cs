using BugsFarm.AnimationsSystem;

namespace BugsFarm.UnitSystem
{
    public class UnitDrinkSystem : UnitNeedSystem
    {
        protected override string PrefixKey { get; } = "Drink_";
        protected override string ItemID { get; } = "4";                         // Предмет воды
        protected override AnimKey ConsumeAnimKey { get; } = AnimKey.Drink; // Анимация приема воды
        protected override string ResourceStatKey { get; } = "stat_consumeWater";
        protected override string NeedTimeStatKey { get; } = "stat_timeWithoutWater";
        protected override string NoNeedTimeStatKey { get; } = "stat_noNeedTimeWater";
    }
}