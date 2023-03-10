using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.AnimationsSystem;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.AudioSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using BugsFarm.UserSystem;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.ChestSystem
{
    public class ChestInteractionCommand : InteractionBaseCommand
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly ISoundSystem _soundSystem;
        private readonly IconLoader _iconLoader;
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly ICurrencyCollectorSystem _currencyCollection;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly ChestDtoStorage _chestDtoStorage;
        private readonly ChestSceneObjectStorage _chestSceneObjectStorage;
        private readonly AnimationModelStorage _animModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly CurrencySettingStorage _currencySettingStorage;
        private readonly ChestModelStorage _modelStorage;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly List<CurrencyItem> _items;

        private const string _contentTakenStatKey = "stat_contentTaken";
        private const string _headerTextKey = "UIChest_{0}";
        private const string _openTextKey = "UIChest_Open";
        private const string _getTextKey = "UIChest_Get";

        private const AnimKey _idleAnimKey = AnimKey.Idle;
        private const AnimKey _idleOpenedAnimKey = AnimKey.Awake;
        private const AnimKey _openAnimKey = AnimKey.Run;
        private ISpineAnimator _animator;
        private AudioModel _audioModel;

        private enum Phase
        {
            Idle,
            Open,
            IdleOpened,
            Close
        }

        private Phase _chestState = Phase.Idle;
        private string _guid;
        
        public ChestInteractionCommand(IUser user,
                                       IUIService uiService, 
                                       ISoundSystem soundSystem,
                                       IconLoader iconLoader,
                                       IInstantiator instantiator,
                                       IInputController<MainLayer> inputController,
                                       ICurrencyCollectorSystem currencyCollection,
                                       SceneEntityStorage sceneEntityController,
                                       ChestDtoStorage chestDtoStorage,
                                       ChestSceneObjectStorage chestSceneObjectStorage,
                                       AnimationModelStorage animModelStorage,
                                       StatsCollectionStorage statsCollectionStorage,
                                       CurrencySettingStorage currencySettingStorage,
                                       ChestModelStorage modelStorage,
                                       AudioModelStorage audioModelStorage)
        {
            _user = user;
            _uiService = uiService;
            _soundSystem = soundSystem;
            _iconLoader = iconLoader;
            _instantiator = instantiator;
            _inputController = inputController;
            _currencyCollection = currencyCollection;
            _sceneEntityController = sceneEntityController;
            _chestDtoStorage = chestDtoStorage;
            _chestSceneObjectStorage = chestSceneObjectStorage;
            _animModelStorage = animModelStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _currencySettingStorage = currencySettingStorage;
            _modelStorage = modelStorage;
            _audioModelStorage = audioModelStorage;
            _items = new List<CurrencyItem>();
        }
        
        public override Task Execute(InteractionProtocol protocol)
        {
            _guid = protocol.Guid;
            _audioModel = _audioModelStorage.Get("UIAudio");
            var window = _uiService.Show<UIChest>();
            _inputController.Lock();
            
            var view = _chestSceneObjectStorage.Get(_guid);
            var dto = _chestDtoStorage.Get(_guid);
            var model = _modelStorage.Get(dto.ModelID);
            var entity = _sceneEntityController.Get(_guid);
            var animModel = _animModelStorage.Get(entity.GetType().Name + "Type_" + model.TypeID);
            
            window.ChangeSkeletonAsset(view.MainSkeleton.skeletonDataAsset);
            window.ChangeGraphicScale(view.MainSkeleton.transform.localScale);
            window.ChangeHeaderLabel(string.Format(_headerTextKey,model.TypeID));
            window.ChangeButtonLabel(_openTextKey);
            window.ChangeButtonActivity(true);
            
            _animator = _instantiator.Instantiate<SpineGraphicAnimator>(new object[] {dto.Guid,window.SpinePlaceHolder,animModel});
            _animator.Initialize();
            _animator.OnAnimationComplete += OnAnimationComplete;
            _animator.SetAnim(_idleAnimKey);
            
            window.ActionButtonEvent += OnButtonActionClickEventHandler;
            return Task.CompletedTask;
        }
        
        private void FillItems()
        {
            if (_guid.IsNullOrDefault() || _items.Count > 0) return;

            var dto = _chestDtoStorage.Get(_guid);
            var model = _modelStorage.Get(dto.ModelID);
            var window = _uiService.Get<UIChest>();

            foreach (var currencyModel in model.Items)
            {
                var item = _instantiator.InstantiatePrefabForComponent<CurrencyItem>
                    (window.CurrencyItemPrfab,
                    window.ContentContainer);
                var settingModel = _currencySettingStorage.Get(currencyModel.ModelID);
                item.SetText(currencyModel.Count.ToString());
                item.SetSprite(_iconLoader.LoadCurrency(currencyModel.ModelID));
                item.SetTextColor(settingModel.ConvertedColor());
                item.SetID(currencyModel.ModelID);
                item.transform.SetAsFirstSibling();
                _items.Add(item);
            }
        }

        private void Dispose()
        {
            _animator.OnAnimationComplete -= OnAnimationComplete;
            
            foreach (var currencyItem in _items)
            {
                Object.Destroy(currencyItem.gameObject);
            }
            
            _items.Clear();
            _uiService.Hide<UIChest>();
            _animator.Dispose();
            _animator = null;
            _inputController.UnLock();
        }

        private void OnButtonActionClickEventHandler(object sender, EventArgs e)
        {
            var window = _uiService.Get<UIChest>();

            switch (_chestState)
            {
                case Phase.Idle:
                    _chestState = Phase.Open;
                    window.ChangeButtonActivity(false);
                    _animator.SetAnim(_openAnimKey);
                    _soundSystem.Play(_audioModel.GetAudioClip("ChestOpen"));
                    break;

                case Phase.IdleOpened:
                    var dto = _chestDtoStorage.Get(_guid);
                    var model = _modelStorage.Get(dto.ModelID);
                    var contentTaken = false;
                    foreach (var currencyItem in _items)
                    {
                        var currModel = model.Items.First(x => x.ModelID == currencyItem.ID);
                        var callback  = contentTaken ? new Action(() => { }) : OnContentTeken;
                        _currencyCollection.Collect(currencyItem.Position,
                            currencyItem.ID,
                            currModel.Count,
                            false,
                            leftCount =>
                            {
                                currencyItem.SetText(leftCount.ToString());
                                var collected = currModel.Count - leftCount;
                                currModel.Count = leftCount;
                                _user.AddCurrency(currModel.ModelID, collected);
                            },
                            callback);
                        contentTaken = true;
                    }

                    _chestState = Phase.Close;
                    window.ChangeButtonActivity(false);
                    break;
            }
        }

        private void OnContentTeken()
        {
            var statCollection = _statsCollectionStorage.Get(_guid);
            statCollection.AddModifier(_contentTakenStatKey, new StatModBaseAdd(1));
            Dispose();
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            var window = _uiService.Get<UIChest>();

            switch (_chestState)
            {
                case Phase.Open:
                    _chestState = Phase.IdleOpened;
                    window.ChangeButtonLabel(_getTextKey);
                    window.ChangeButtonActivity(true);
                    _animator.SetAnim(_idleOpenedAnimKey);
                    FillItems();
                    break;
            }
        }
    }
}