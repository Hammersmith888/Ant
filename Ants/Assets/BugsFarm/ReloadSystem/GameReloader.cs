using System;
using BugsFarm.App;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.UIService;
using BugsFarm.SimulatingSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace BugsFarm.ReloadSystem
{
    public class GameReloader
    {
        public static bool IsReloading => _isReloading;
        
        private static bool _isReloading;

        private readonly SimulatingCenter _simulatingCenter;
        private readonly ISavableStorage _savableStorage;
        private readonly IInstantiator _instantiator;
        private readonly AppInstaller _appInstaller;
        private readonly ISaveManager _saveManager;
        private readonly IUIService _uiService;
        private AsyncOperation _unloadHandle;
        private readonly UIRoot _uiRoot;
        private readonly IUser _user;

        private string _projectContextPath = "ProjectContext";

        public GameReloader(IInstantiator instantiator,
                            ISaveManager saveManager,
                            IUser user,
                            ISavableStorage savableStorage,
                            SimulatingCenter simulatingCenter,
                            AppInstaller appInstaller,
                            UIRoot uiRoot,
                            IUIService uiService)
        {
            _uiRoot = uiRoot;
            _simulatingCenter = simulatingCenter;
            _savableStorage = savableStorage;
            _uiService = uiService;
            _appInstaller = appInstaller;
            _user = user;
            _saveManager = saveManager;
            _instantiator = instantiator;
            Application.focusChanged += OnFocusChanged;
            
        }
        private void OnFocusChanged(bool isFocused)
        {
            if (isFocused && _simulatingCenter.GetFocusDelta() > 10.0d)
            {
                ReloadGame();
            } 
        }
        public void ReloadGame()
        {
            _isReloading = true;
            
            Application.focusChanged -= OnFocusChanged;
            _simulatingCenter.Dispose();
            _saveManager.Save(JsonUtility.ToJson(_user.Dto), PathConstants.GetUserPath(_user.Id));
            _saveManager.SaveAll(PathConstants.GetUserDataPath(_user.Id));
            _savableStorage.Clear();
            MessageBroker.Default.Publish(new GameReloadingReport());
            _instantiator.Instantiate<ReloadGameObjectsDestroyer>().DestroyObjectsOnScene();
            _uiService.HideAll();
            _unloadHandle = SceneManager.UnloadSceneAsync("Farm");
            _unloadHandle.completed += OnSceneUnloaded;
            
        }

        private void OnSceneUnloaded(AsyncOperation operation)
        {
            _unloadHandle.completed -= OnSceneUnloaded;
            _instantiator.Instantiate<StorageCleaner>().ClearStorages();

            _isReloading = false;
            
            GameObject.Destroy(_appInstaller.gameObject);
            DOTween.Clear();
            MessageBroker.Default.Dispose();
            SceneManager.LoadScene("Init");
        }
    }
}

