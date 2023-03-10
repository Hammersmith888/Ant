using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class SpawnFromAlphaTask : SpawnUnitTask
    {
        private readonly IInstantiator _instantiator;
        private ITask _fadeInOutTask;
        public SpawnFromAlphaTask(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        public override void Execute(params object[] args)
        {
            if(IsRunned) return;
            base.Execute(args);
            View.SetActive(true);
            View.SetAlpha(0);
        }

        protected override void OnSpawned()
        {
            _fadeInOutTask = _instantiator.Instantiate<FadeInOutUnitTask>( new object[]{false});
            _fadeInOutTask.OnComplete += _ => Completed();
            _fadeInOutTask.Execute(UnitId);
        }

        protected override void OnDisposed()
        {
            _fadeInOutTask?.Interrupt();
            _fadeInOutTask = null;
            base.OnDisposed();
        }
    }
}