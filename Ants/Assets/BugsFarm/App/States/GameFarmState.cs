using BugsFarm.AudioSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.StateMachine;

namespace BugsFarm.App.States
{
    public class GameFarmState : State
    {
        private readonly IMusicSystem _musicSystem;
        private readonly InteractorStorage _interactorStorage;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly AudioModel _audioModel;

        public GameFarmState(IMusicSystem musicSystem,
                             InteractorStorage interactorStorage,
                             AudioModelStorage audioModelStorage) : base("Farm")
        {
            _musicSystem = musicSystem;
            _interactorStorage = interactorStorage;
            _audioModel = audioModelStorage.Get("BackgroundMusic");
        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            var headerInteractor = _interactorStorage.Get("UIHeaderInteractor");
            var bottomInteractor = _interactorStorage.Get("UIBottomInteractor");
            var dayTimeInteractor = _interactorStorage.Get("DayTimeInteractor");
            headerInteractor.Init();
            bottomInteractor.Init();
            dayTimeInteractor.Init();
            _musicSystem.SetVolume(1f);
            _musicSystem.PlayTransition(_audioModel.GetAudioClip("Farm"));
        }

        public override void OnExit()
        {
            base.OnExit();
            var headerInteractor  = _interactorStorage.Get("UIHeaderInteractor");
            var bottomInteractor  = _interactorStorage.Get("UIBottomInteractor");
            var dayTimeInteractor = _interactorStorage.Get("DayTimeInteractor");
            bottomInteractor.Dispose();
            dayTimeInteractor.Dispose();
            headerInteractor.Dispose();
            _musicSystem.StopWithFading();
        }
    }
}