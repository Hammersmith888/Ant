using System;
using BugsFarm.Logger;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UserSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.App
{
    public class GameLoader
    {
        private readonly IInstantiator _instantiator;
        private readonly IBootstrap _bootstrap;
        private readonly IUIService _uiService;
        private readonly IUserInternal _userController;

        private const string _sceneLoadName = "Farm";
        private const string _contextName = "FarmSceneContext";
        private const float _delayShowPopupWindow = 1f;
        private const float _delayProgressStart = 1f;
        
        private UILoader _loaderWindow;
        private IDisposable _delayEvent;
        private BugsLogger _logger;

        public GameLoader(IInstantiator instantiator,
                          IBootstrap bootstrap,
                          IUIService uiService,
                          IUserInternal userController)
        {
            _instantiator = instantiator;
            _bootstrap = bootstrap;
            _uiService = uiService;
            _userController = userController;
        }

        public void Init()
        {

            _loaderWindow = _uiService.Show<UILoader>();
            _logger = _instantiator.Instantiate<BugsLogger>();
            _logger.Initialize();
            // TODO : API, Auth, User (load / create)
            _bootstrap.AddCommand(_instantiator.Instantiate<LocalizationInitCommand>());
            _bootstrap.AddCommand(_instantiator.Instantiate<CreateUserCommand>(new object[]{"defaultUser"}));
            _bootstrap.AllCommandsDone += Authorized;
            _bootstrap.StartExecute();
        }

        private void Authorized(object sender, EventArgs e)
        {
            _bootstrap.AllCommandsDone -= Authorized;
            if (!_userController.Dto.AcceptedPPA) // todo make it command
            {
                Delay(_delayShowPopupWindow, () =>
                {
                    var userAgreement = _instantiator.Instantiate<UserAgreementInteractor>(); // todo make it command
                    userAgreement.Initialize(() =>
                    {
                        _userController.Dto.AcceptedPPA = true;
                        ExecuteFirstPassBootstrap();
                    });
                });
            }
            else
            {
                ExecuteFirstPassBootstrap();
            }
        }

        private void ExecuteFirstPassBootstrap()
        {
            Delay(_delayProgressStart, () =>
            {
                _loaderWindow.ShowProgress();
                _bootstrap.AllCommandsDone += ExecuteSecondPassBootstrap;
                _bootstrap.AddCommand(_instantiator.Instantiate<LoadSceneCommand>(new object[] {_sceneLoadName}));
                _bootstrap.AddCommand(_instantiator.Instantiate<WaitContextInstallCommand>(new object[] {_contextName}));
                _bootstrap.StartExecute();
            });
        }
        
        private void ExecuteSecondPassBootstrap(object sender, EventArgs e)
        {
            _bootstrap.AllCommandsDone -= ExecuteSecondPassBootstrap;
            _bootstrap.ProgressUpdate  += OnProgressUpdate;
            _bootstrap.AllCommandsDone += OnAllCommandsDone;
            _bootstrap.AddCommand(_instantiator.Instantiate<InitFarmCommand>(new object[] {_contextName, _bootstrap}));
            _bootstrap.StartExecute();
        }
        
        private void Delay(float delay, Action onCompelte = null)
        {
            _delayEvent = Observable.Timer(TimeSpan.FromSeconds(delay))
                 .Subscribe(_ => DelayCompleted(onCompelte));
        }

        private void DelayCompleted(Action onCompelte = null)
        {
            onCompelte?.Invoke();
            _delayEvent.Dispose();
        }
        
        private void OnProgressUpdate(object task, float progress)
        {
            if (!_loaderWindow)
            {
                Debug.LogError($"{this} : {nameof(UILoader)} does not exits");
                return;
            }
            _loaderWindow.SetProgress(progress);
        }

        private void OnAllCommandsDone(object sender, EventArgs e)
        {
            _bootstrap.AllCommandsDone -= OnAllCommandsDone;
            _bootstrap.ProgressUpdate  -= OnProgressUpdate;
            _loaderWindow.HideProgress();
            _uiService.Hide<UILoader>();
            _instantiator.Instantiate<GameApp>();
            MessageBroker.Default.Publish(new GameStateChangeRequest("Farm"));
        }
    }
}