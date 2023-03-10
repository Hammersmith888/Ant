namespace BugsFarm.UnitSystem
{
    public class Spider : UnitCrawlBase
    {
        protected override string SpeakerId => base.SpeakerId + StatsCollection.GetValue(_stageStatKey);
        private const string _stageStatKey = "stat_stage";
    }
}