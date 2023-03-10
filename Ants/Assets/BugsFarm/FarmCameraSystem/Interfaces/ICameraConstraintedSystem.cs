using System;
using Zenject;

namespace BugsFarm.FarmCameraSystem
{
    public interface ICameraConstraintedSystem : IInitializable, IDisposable
    {
        bool AllowConstraints { get; }
        void SetAllowConstraints(bool allow);
    }
}