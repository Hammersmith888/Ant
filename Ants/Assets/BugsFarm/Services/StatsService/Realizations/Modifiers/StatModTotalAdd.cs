namespace BugsFarm.Services.StatsService
{
    public class StatModTotalAdd : StatModifier
    {
        public override int Order => 4;
        public override float ApplyModifier(Stat stat, float modValue)
        {
            return modValue;
        }

        public StatModTotalAdd(float value, bool stacks = true, string modifierId = "") :
            base(value, stacks, modifierId)
        {
        }
    }
}