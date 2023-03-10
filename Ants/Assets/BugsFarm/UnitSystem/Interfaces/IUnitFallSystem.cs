namespace BugsFarm.UnitSystem
{
    public interface IUnitFallSystem
    {
        void Registration(string unitGuid);
        void UnRegistration(string unitGuid);
        bool HasEntity(string unitGuid);
        bool IsFall(string unitGuid);
        void OnLoseGround(string unitGuid);
    }
}