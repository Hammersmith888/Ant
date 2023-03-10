using Com.LuisPedroFonseca.ProCamera2D;

namespace BugsFarm.FarmCameraSystem
{
    public class FarmTriggerBoundaries : ProCamera2DTriggerBoundaries
    {
        protected override void Awake()
        {
            base.Awake();
            Start(); // Force init immediately
        }
    }
}