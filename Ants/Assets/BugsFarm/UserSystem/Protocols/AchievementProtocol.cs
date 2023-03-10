using BugsFarm.Services.CommandService;

namespace BugsFarm.UserSystem
{
    public enum Achievement
    {
        FightedBoss,
        LevelUp,
        QuestDone,
        ArenaWin,
        NewSeason,
    }
    public struct AchievementProtocol : IProtocol
    {
        public Achievement Id;
    }
}