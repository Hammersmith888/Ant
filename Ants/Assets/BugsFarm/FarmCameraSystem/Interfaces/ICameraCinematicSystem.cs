using System;

namespace BugsFarm.FarmCameraSystem
{
    public interface ICameraCinematicSystem
    {
        event Action OnStart;
        event Action OnTargetStay;
        event Action OnComplete;
        void EndCinematic();
    }
}