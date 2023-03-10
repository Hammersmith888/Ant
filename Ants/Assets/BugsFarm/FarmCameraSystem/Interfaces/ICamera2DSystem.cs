namespace BugsFarm.FarmCameraSystem
{
    public interface ICamera2DSystem
    {
        bool Enabled { get; }
        bool FollowedVertical { get; }
        bool FollowedHorizontal { get; }
        void Reset();
        void ResetMovement();
        void Enable(bool enable);
        void FollowVertical(bool allow);
        void FollowHorizontal(bool allow);
    }
}