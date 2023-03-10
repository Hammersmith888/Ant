using System;
using System.Linq;
using BugsFarm.AudioSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class UIBottomInteractor : IInteractorService
    {
        public string Id => "UIBottomInteractor";

        private readonly UIRoot _uiRoot;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IStateMachine _stateMachine;
        private readonly ISoundSystem _soundSystem;
        private readonly AudioModelStorage _audioModelStorage;
        private AudioModel _audioModel;
        private BattlePass _battlePass;
        private IDisposable _donatShopEvent;

        private const string _donateState = "DonatShop";

        public UIBottomInteractor(UIRoot uiRoot,
                                  IUIService uiService,
                                  IInstantiator instantiator,
                                  IInputController<MainLayer> inputController,
                                  IStateMachine stateMachine,
                                  ISoundSystem soundSystem,
                                  AudioModelStorage audioModelStorage)
        {
            _uiRoot = uiRoot;
            _uiService = uiService;
            _instantiator = instantiator;
            _inputController = inputController;
            _stateMachine = stateMachine;
            _soundSystem = soundSystem;
            _audioModelStorage = audioModelStorage;
        }

        public void Init()
        {
            _audioModel = _audioModelStorage.Get("UIAudio");
            var args = new object[] {_stateMachine};
            _stateMachine.Add(_instantiator.Instantiate<FarmShopState>(args));
            _stateMachine.Add(_instantiator.Instantiate<MyBugsState>(args));
            _stateMachine.Add(_instantiator.Instantiate<DonatShopState>(args.Append(_donateState)));
            _stateMachine.Add(_instantiator.Instantiate<BattleState>(args));
            
            var window = _uiService.Show<UIBottomWindow>();
            window.ButtonClickedEvent += ButtonClickedEvent;
            
            _battlePass = _instantiator.Instantiate<BattlePass>();
            _battlePass.OnValidate += OnBattlePassValidate;
            _battlePass.Validate();

            _donatShopEvent = MessageBroker.Default.Receive<SwitchDonatShopProtocol>().Subscribe(OnDonateShopEventHandler);
        }

        public void Dispose()
        {
            
            if(_battlePass != null)
                _battlePass.OnValidate -= OnBattlePassValidate;
            _uiService.Hide<UIBottomWindow>();
            _stateMachine.Current.OnExit();
            _stateMachine.Clear();
            _battlePass?.Dispose();
            _battlePass = null;
            _donatShopEvent?.Dispose();
            _donatShopEvent = null;
        }

        private void PlaySoundClick()
        {
            _soundSystem.Play(_audioModel.GetAudioClip("TabSwitch"));
        }

        private void ButtonClickedEvent(object sender, string stateId)
        {
            if (_stateMachine.Current.Id == "Empty" && _inputController.Locked)
            {
                return;
            }
            
            PlaySoundClick();
            if (_stateMachine.Current.Id == stateId)
            {
                _stateMachine.Exit(stateId);
            }
            else
            {
                _stateMachine.Switch(stateId);
            }
        }
        
        private void OnBattlePassValidate(bool validated)
        {
            var window = _uiService.Get<UIBottomWindow>();
            
            if (window.gameObject.activeSelf)
            {
                window.SetInteractable("Battle", validated);
            }
        }
        
        private void OnDonateShopEventHandler(SwitchDonatShopProtocol obj)
        {
            if (_stateMachine.Current.Id != _donateState)
            {
                _stateMachine.Switch(_donateState);
            }
            else
            {
                _stateMachine.Exit(_donateState);
            }
        }
    }
}