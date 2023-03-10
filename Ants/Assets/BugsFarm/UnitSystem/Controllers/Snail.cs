namespace BugsFarm.UnitSystem
{
    public class Snail : UnitCrawlBase
    {
        protected override string SpeakerId => base.SpeakerId + StatsCollection.GetValue(_stageStatKey);
        private const string _stageStatKey = "stat_stage";
    }
}