using System;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulatingSystem;
using UniRx;

namespace BugsFarm.UI
{
    public class UISimulationInteractor
    {
        private readonly IUIService _uiService;
        private readonly SimulatingCenter _simulatingCenter;
        private readonly IInputController<MainLayer> _inputController;
        private readonly ISimulationSystem _simulationSystem;
        private readonly GameReloader _gameReloader;
        private double _simulateTime = 60 * 60;
        private bool _willbeLocked;
        private IDisposable _reloadingEvent;

        public UISimulationInteractor(IUIService uiService,
                                      IInputController<MainLayer> inputController,
                                      ISimulationSystem simulationSystem,
                                      SimulatingSystem.SimulatingCenter simulatingCenter,
                                      GameReloader gameReloader)
        {
            _gameReloader = gameReloader;
            _simulatingCenter = simulatingCenter;
            _uiService = uiService;
            _inputController = inputController;
            _simulationSystem = simulationSystem;
        }

        public void Initialize()
        {
            _uiService.Show<UISimulationButtonWindow>().ClickEvent += WindowOpenClickEvent;
            _reloadingEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);
            SetFormatTime();
        }

        private void OnGameReloading(GameReloadingReport report)
        {
            var buttonWidow = _uiService.Get<UISimulationButtonWindow>();
            buttonWidow.ClickEvent -= WindowOpenClickEvent;
            
            var window = _uiService.Get<UISimulationWindow>();
            _uiService.Hide<UISimulationWindow>();
            _uiService.Hide<UISimulationButtonWindow>();
            
            window.HoursClickEvent -= HoursClickEvent;
            window.MinsClickEvent -= MinsClickEvent;
            window.SecondClickEvent -= SecondClickEvent;
            window.SimulateClickEvent -= SimulateClickEvent;
            window.ResetClickEvent -= ResetClickEvent;
            window.CloseClickEvent -= WindowCloseClickEvent;
            _reloadingEvent.Dispose();

            _inputController.UnLock();
            
        }
        private void SetFormatTime()
        {
            _uiService.Get<UISimulationWindow>().SetTimeText(Format.Time(TimeSpan.FromSeconds(_simulateTime)));
        }

        private void HoursClickEvent(object sender, EventArgs e)
        {
            _simulateTime += 60d * 60d;
            SetFormatTime();
        }

        private void MinsClickEvent(object sender, EventArgs e)
        {
            _simulateTime += 60d;
            SetFormatTime();
        }

        private void SecondClickEvent(object sender, EventArgs e)
        {
            _simulateTime += 1d;
            SetFormatTime();
        }

        private void SimulateClickEvent(object sender, EventArgs e)
        {
            _simulatingCenter.SetSimulationTime((int)_simulateTime);
            _gameReloader.ReloadGame();
        }

        private void ResetClickEvent(object sender, EventArgs e)
        {
            _simulateTime = 0;
            SetFormatTime();
        }

        private void WindowCloseClickEvent(object sender, EventArgs e)
        {
            var window = _uiService.Get<UISimulationWindow>();
            _uiService.Hide<UISimulationWindow>();
            _uiService.Show<UISimulationButtonWindow>();

            window.HoursClickEvent -= HoursClickEvent;
            window.MinsClickEvent -= MinsClickEvent;
            window.SecondClickEvent -= SecondClickEvent;
            window.SimulateClickEvent -= SimulateClickEvent;
            window.ResetClickEvent -= ResetClickEvent;
            window.CloseClickEvent -= WindowCloseClickEvent;
            
            if (_willbeLocked) return;
            _inputController.UnLock();
        }

        private void WindowOpenClickEvent(object sender, EventArgs e)
        {
            var window = _uiService.Show<UISimulationWindow>();
            _willbeLocked = _inputController.Locked;
            _inputController.Lock();
            window.HoursClickEvent += HoursClickEvent;
            window.MinsClickEvent += MinsClickEvent;
            window.SecondClickEvent += SecondClickEvent;
            window.SimulateClickEvent += SimulateClickEvent;
            window.ResetClickEvent += ResetClickEvent;
            window.CloseClickEvent += WindowCloseClickEvent;
            _uiService.Hide<UISimulationButtonWindow>();
        }
    }
}