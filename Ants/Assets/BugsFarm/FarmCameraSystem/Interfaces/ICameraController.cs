using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    public interface ICameraController
    {
        Camera Camera { get; }
        ICameraCinematicSystem Cinematic { get; }
    }
}