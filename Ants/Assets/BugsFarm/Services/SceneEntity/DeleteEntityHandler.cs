namespace BugsFarm.Services.SceneEntity
{
    public class DeleteEntityHandler
    {
        public DeleteEntityHandler(SceneEntityStorage entityController, string guid)
        {
            if (!entityController.HasEntity(guid)) return;
            if (entityController.Get(guid) is IDisposableHandler disposableHandler)
            {
                disposableHandler.OnDisposeHandle();
            }
        }
    }
}