using System;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.AudioSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using UnityEngine;
using Random = UnityEngine.Random;
using Zenject;
using BugsFarm.SimulationSystem;


namespace BugsFarm.UI
{
    public class BoubbleInteractor : ITickable
    {
        private string[] Sounds { get; } = {"PopupTapped1", "PopupTapped2"};

        private readonly IMonoPool _monoPool;
        private readonly UIWorldRoot _uiWorldRoot;
        private readonly ISoundSystem _soundSystem;
        private readonly IconLoader _iconLoader;
        private readonly ITickableManager _itickbleManager;
        private readonly AudioModel _audioModel;
        private readonly Transform _target;
        private UIBubbleCurrency _popup;
        private Action _onTapped;
        private IDisposable _moveEvent;
        private bool _finalized;
        private StatVital _resourceStat;


        public BoubbleInteractor(Transform target,
                                 IMonoPool monoPool,
                                 ITickableManager itickbleManager,
                                 UIWorldRoot uiWorldRoot,
                                 ISoundSystem soundSystem,
                                 AudioModelStorage audioModelStorage,
                                 IconLoader iconLoader)
        {
            _target = target;
            _monoPool = monoPool;
            _uiWorldRoot = uiWorldRoot;
            _soundSystem = soundSystem;
            _iconLoader = iconLoader;
            _itickbleManager = itickbleManager;
            _audioModel = audioModelStorage.Get("UIAudio");
        }

        public void Init()
        {
            if (_finalized || _popup || !_target)
            {
                return;
            }
            
            _popup = _monoPool.Spawn<UIBubbleCurrency>(_uiWorldRoot.Transform);
            _popup.OnTapped += () => _onTapped?.Invoke();
            _popup.ChangePosition(_target.position);
            _popup.ChangeInteraction(true);
            _itickbleManager.Add(this);
        }
        
        public void SetActionTap(Action onTapped)
        {
            if (_finalized)
            {
                return;
            }
            _onTapped = onTapped;
        }

        public void SetCurrency(string currencyId)
        {
            if (_finalized)
            {
                return;
            }
            _popup.SetIco(_iconLoader.LoadCurrency(currencyId));
        }

        public void Update(string text = null)
        {
            if (_finalized || !_popup || !_target)
            {
                return;
            }
            
            _popup.SetText(text);
            _popup.ChangePosition(_target.position);
        }

        public void Dispose()
        {
            if (_finalized) return;
            if (_popup)
            {
                var sound = _audioModel.GetAudioClip(Sounds[Random.Range(0, Sounds.Length)]);
                _soundSystem.Play(_popup.transform.position, sound);
                _monoPool.Despawn(_popup);
                _popup = null;
                _moveEvent?.Dispose();
                _moveEvent = null;
                _itickbleManager.Remove(this);
            }

            _onTapped = null;
            _finalized = true;
        }

        public void Tick()
        {
            var value = (int)(_resourceStat.CurrentValue);

            _popup.SetCoinValue(value.ToString());
        }
        internal void SetCoins(StatVital resourceStat)
        {
            _resourceStat = resourceStat;
        }
    }
}