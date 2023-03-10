using BugsFarm.BuildingSystem;
using BugsFarm.DayTimeSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.UIService;
using Zenject;

namespace BugsFarm.UI
{
    public class UIFarmInitCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;
        private readonly InteractorStorage _interactorStorage;
        private readonly AntHillTaskHandler _antHillTaskHandler;

        public UIFarmInitCommand(
            IInstantiator instantiator,
            IUIService uiService,
            InteractorStorage interactorStorage,
            AntHillTaskHandler antHillTaskHandler)
        {
            _antHillTaskHandler = antHillTaskHandler;
            _instantiator = instantiator;
            _uiService = uiService;
            _interactorStorage = interactorStorage;
        }
        
        public override void Do()
        { 
            var headerInteractor = _instantiator.Instantiate<UIHeaderInteractor>();
            _interactorStorage.Add(headerInteractor);
            
            var bottomInteractor = _instantiator.Instantiate<UIBottomInteractor>();
            _interactorStorage.Add(bottomInteractor);

            var dayTimeInteractor = _instantiator.Instantiate<DayTimeInteractor>();
            _interactorStorage.Add(dayTimeInteractor);
            
            _antHillTaskHandler.Initialize();
            _instantiator.Instantiate<UISimulationInteractor>().Initialize();
            _instantiator.Instantiate<UIDebugTasksInteractor>().Initialize();
            _instantiator.Instantiate<UIDebugAssignTaskInteractor>().Initialize();
            _instantiator.Instantiate<UIDebugPlaceNumInteractor>().Initialize();
            _instantiator.Instantiate<UIDebugLogsInteractor>().Initialize();
            _instantiator.Instantiate<UIQuestInteractor>().Initialize();
            _instantiator.Instantiate<UIDebugAntKiller>().Initialize();
            //_uiService.Show<UIQuestsButton>();
            //_uiService.Show<UIDebugReloadButtonWindow>();
            OnDone();
        }
    }
}