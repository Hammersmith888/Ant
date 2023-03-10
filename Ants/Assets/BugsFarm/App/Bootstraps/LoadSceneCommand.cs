using BugsFarm.Services.BootstrapService;
using UnityEngine.SceneManagement;
using AsyncOperation = UnityEngine.AsyncOperation;

namespace BugsFarm.App
{
    public class LoadSceneCommand : Command
    {
        private readonly string _sceneName;
        private readonly LoadSceneMode _loadSceneMode;

        public LoadSceneCommand(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Additive)
        {
            _sceneName = sceneName;
            _loadSceneMode = loadSceneMode;
        }

        public override void Do()
        {
            var asyncOperation = SceneManager.LoadSceneAsync(_sceneName, _loadSceneMode);
            asyncOperation.completed += OnLoadSceneComplete;
            asyncOperation.allowSceneActivation = true;
        }

        private void OnLoadSceneComplete(AsyncOperation operation)
        {
            operation.completed -= OnLoadSceneComplete;
            var targetScene = SceneManager.GetSceneByName(_sceneName);
            SceneManager.SetActiveScene(targetScene);
            OnDone();
        }
    }
}