using System.Linq;
using BugsFarm.Services.InputService;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UI
{
    public class UIDebugTasksInteractor : ITickable
    {
        private readonly TickableManager _tickableManager;
        private readonly IUIService _uiService;
        private readonly UIRoot _uiRoot;
        private readonly IInputController<MainLayer> _inputController;
        private readonly TaskStorage _taskStorage;
        private bool _initialized;
        private bool _willbeLocked;
        public UIDebugTasksInteractor(TickableManager tickableManager,
                                    IUIService uiService,
                                    UIRoot uiRoot,
                                    IInputController<MainLayer> inputController,
                                    TaskStorage taskStorage)
        {
            _tickableManager = tickableManager;
            _uiService = uiService;
            _uiRoot = uiRoot;
            _inputController = inputController;
            _taskStorage = taskStorage;
        }

        public void Initialize()
        {
            if(_initialized) return;
            _initialized = true;
            var windowButton = _uiService.Show<UITaskStorageDebugButtonWindow>(_uiRoot.MiddleContainer);
            windowButton.OnClick += WindowButtonOnClick;
        }

        private void WindowButtonOnClick()
        {
            if(!_initialized) return;
            _willbeLocked = _inputController.Locked;
            _inputController.Lock();
            _uiService.Hide<UITaskStorageDebugButtonWindow>();
            var window = _uiService.Show<UITaskStorageDebugWindow>(_uiRoot.MiddleContainer);
            window.OnClose += OnWindowClose;
            _tickableManager.Add(this);
        }

        private void OnWindowClose()
        {
            if(!_initialized) return;
            _uiService.Hide<UITaskStorageDebugWindow>();
            _uiService.Show<UITaskStorageDebugButtonWindow>(_uiRoot.MiddleContainer);
            _tickableManager.Remove(this);
            
            if (_willbeLocked) return;
            _inputController.UnLock();
        }

        public void Dispose()
        {
            if(!_initialized) return;
            _initialized = false;
            _uiService.Hide<UITaskStorageDebugWindow>();
            _uiService.Hide<UITaskStorageDebugButtonWindow>();
        }
        
        public void Tick()
        {
            if(!_initialized) return;
            var tasks = _taskStorage.GetAllInfo();
            if(tasks == null) return;
            var infos = tasks.Select(x => "<color=red>TaskName</color> : " + x.TaskName + "\n=======================================");
            var buildedInfo = infos.Aggregate("", (current, info) => current + (info + "\n"));
            _uiService.Get<UITaskStorageDebugWindow>().SetText(buildedInfo);
        }
    }
}