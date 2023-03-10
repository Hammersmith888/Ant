using BugsFarm.TaskSystem;

namespace BugsFarm.UnitSystem
{
    public class RotateToTask : BaseTask
    {
        private readonly bool _lookLeft;
        private readonly UnitMoverStorage _moverStorage;

        public RotateToTask(bool lookLeft, UnitMoverStorage moverStorage)
        {
            _lookLeft = lookLeft;
            _moverStorage = moverStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            
            var unitGuid = (string) args[0];
            if (_moverStorage.HasEntity(unitGuid))
            {
                var mover = _moverStorage.Get(unitGuid);
                mover.SetLook(_lookLeft);
            }
            Completed();
        }
    }
}