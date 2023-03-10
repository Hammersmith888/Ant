namespace BugsFarm.RoomSystem
{
    public class RoomAnimatedController : RoomBaseController
    {
        protected override void OnRoomOpened(string guid)
        {
            if (Id != guid)
            {
                return;
            }
            base.OnRoomOpened(guid);
            SetVisible(true);
        }
    }
}