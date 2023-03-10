using BugsFarm.AnimationsSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitFlyableBase : UnitBase
    {
        public override void Initialize()
        {
            if(Finalized) return;
            var animatorProtocol = new CreateAnimatorProtocol(Id, GetType().Name, UnitSceneObjectStorage.Get(Id).MainSkeleton);
            Instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animatorProtocol);
            Instantiator.Instantiate<CreateMoverCommand<UnitFlayableMover>>().Execute(new CreateMoverProtocol(Id));
            base.Initialize();
        }

        public override void Dispose()
        {
            if (Finalized) return;
            var moverProtocol = new DeleteMoverProtocol(Id);
            Instantiator.Instantiate<DeleteMoverCommand>().Execute(moverProtocol);
            
            var animatorProtocol = new RemoveAnimatorProtocol(Id);
            Instantiator.Instantiate<RemoveAnimatorCommand>().Execute(animatorProtocol);
            base.Dispose();
        }
    }
}