using BugsFarm.Services.StatsService;

namespace BugsFarm.TaskSystem
{
    public class TimerFromStatTask : SimulatedTimerTask
    {
        private StatVital _timerStat;
        
        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            _timerStat = (StatVital) args[0];
            base.Execute(_timerStat.CurrentValue);
        }
        
        protected override void OnDisposed()
        {
            _timerStat = null;
        }
    }
}