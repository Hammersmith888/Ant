using UnityEngine;

namespace devtodev.WSA
{
    public static class PushActionHandler
    {
        /// <summary>
        /// Method to try handle push action if it pass self arguments in startup.
        /// It method need to handle push action if SDK not activated yet.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HandlePushAction()
        {
#if !UNITY_EDITOR && UNITY_WSA
            if (!Metro.Push.Logic.PushActionsHandler.IsInitialized)
            {
                Metro.Push.Logic.PushActionsHandler.Initialize();
            }
#endif
        }
    }
}
