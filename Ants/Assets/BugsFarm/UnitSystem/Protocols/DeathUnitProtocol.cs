using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public struct DeathUnitProtocol : IProtocol
    {
        public string UnitId;
        public string DeathReason;
    }

    public static class DeathReason
    {
        public const string Fighted = "fighted";
        public const string Food = "timeWithoutFood";
        public const string Water = "timeWithoutWater";
        public const string Sleep = "timeWithoutSleep";
        public const string Hospital = "hospitalRemove";
    }
}