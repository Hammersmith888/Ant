using BugsFarm.Services.BootstrapService;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugsFarm.App
{
    public class UnLoadSceneCommand : Command
    {
        private readonly string _sceneName;

        public UnLoadSceneCommand(string sceneName)
        {
            _sceneName = sceneName;
        }

        public override void Do()
        {
            var asyncOperation = SceneManager.UnloadSceneAsync(_sceneName);
            asyncOperation.completed += OnUnLoadSceneComplete;
        }

        private void OnUnLoadSceneComplete(AsyncOperation operation)
        {
            operation.completed -= OnUnLoadSceneComplete;
            OnDone();
        }
    }
}