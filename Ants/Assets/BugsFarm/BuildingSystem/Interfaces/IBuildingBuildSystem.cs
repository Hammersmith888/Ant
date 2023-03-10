using System;

namespace BugsFarm.BuildingSystem
{
    public interface IBuildingBuildSystem
    {
        event Action<string> OnCompleted;
        event Action<string> OnStarted;
        void Registration(string guid);
        void UnRegistration(string guid);
        void Start(string buildingGuid);
        bool HasEntity(string guid);
        bool IsBuilding(string guid);
        void ResetBuildingTime(string guid);
        bool CanBuild(string guid);
        void ForceComplete(string guid);
        bool GetTime(string guid, out float current, out float maximum);
    }
}