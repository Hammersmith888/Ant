using BugsFarm.AnimationsSystem;

namespace BugsFarm.UnitSystem
{
    public class UnitAntBase : UnitBase
    {
        public override void Initialize()
        {
            if(Finalized) return;
            var view = (AntSceneObject)UnitSceneObjectStorage.Get(Id);
            var spineMain = view.MainSkeleton;
            var spineClimb = view.ClimbSkeleton;
            var animatorProtocol = new CreateAnimatorProtocol(Id, GetType().Name, spineClimb, spineMain);
            Instantiator.Instantiate<CreateAnimatorCommand<UnitAntAnimator>>().Execute(animatorProtocol);
            Instantiator.Instantiate<CreateMoverCommand<UnitAntMover>>().Execute(new CreateMoverProtocol(Id));
            base.Initialize();
        }

        public override void Dispose()
        {
            if(Finalized) return;
            var moverProtocol = new DeleteMoverProtocol(Id);
            Instantiator.Instantiate<DeleteMoverCommand>().Execute(moverProtocol);
            
            var animatorProtocol = new RemoveAnimatorProtocol(Id);
            Instantiator.Instantiate<RemoveAnimatorCommand>().Execute(animatorProtocol);
            base.Dispose();
        }
    }
}