namespace BugsFarm.FarmCameraSystem
{
    public interface ICameraPanSystem
    {
        bool AllowPan { get; }
        void CenterPanTargetOnCamera();
        void SetAllowPan(bool allowPan);
    }
}