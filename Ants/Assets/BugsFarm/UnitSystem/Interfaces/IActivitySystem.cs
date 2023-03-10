using System;

namespace BugsFarm.UnitSystem
{
    public interface IActivitySystem
    {
        event Action<string> OnStateChanged;
        void Registration(ActivitySystemProtocol protocol);
        void UnRegistration(string guid);
        void Activate(string guid, bool activate, bool force = false);
        bool IsActive(string guid);
        bool HasEntity(string guid);
    }
}