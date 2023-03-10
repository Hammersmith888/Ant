using System;
using System.Collections.Generic;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.AudioSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.FarmCameraSystem;
using BugsFarm.Quest;
using BugsFarm.Services.InputService;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using Spine;
using Spine.Unity;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class VirtualChestInteractor
    {
        private readonly ICurrencyCollectorSystem _currencyCollectorSystem;
        private readonly CurrencySettingStorage _currencySettingStorage;
        private readonly FarmCameraSceneObject _farmCameraSceneObject;
        private readonly IInputController<MainLayer> _inputController;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly IInstantiator _instantiator;
        private readonly ISoundSystem _soundSystem;
        private readonly IconLoader _iconLoader;
        private readonly IUIService _uiService;
        private readonly IUser _user;

        private VirtualChestDto _virtualChestDto;
        private AudioModel _audioModel;
        private List<CurrencyItem> _items;

        private const string _headerTextKey = "UIChest_{0}";
        private const string _openTextKey = "UIChest_Open";
        private const string _getTextKey = "UIChest_Get";
        
        private const string _idleAnimKey = "Idle";
        private const string _openingAnimKey = "Open";
        private const string _openedAnimKey = "Idle open";
        
        public VirtualChestInteractor(IInstantiator instantiator,
            IInputController<MainLayer> inputController,
            ICurrencyCollectorSystem currencyCollectorSystem,
            FarmCameraSceneObject farmCameraSceneObject,
            CurrencySettingStorage currencySettingStorage,
            IconLoader iconLoader,
            ISoundSystem soundSystem,
            AudioModelStorage audioModelStorage,
            IUser user,
            IUIService uiService)
        {
            _uiService = uiService;
            _user = user;
            _iconLoader = iconLoader;
            _audioModelStorage = audioModelStorage;
            _currencySettingStorage = currencySettingStorage;
            _inputController = inputController;
            _currencyCollectorSystem = currencyCollectorSystem;
            _farmCameraSceneObject = farmCameraSceneObject;
            _instantiator = instantiator;
            _soundSystem = soundSystem;
            _items = new List<CurrencyItem>();
        }

        public void Execute(VirtualChestDto virtualChestDto)
        {
            _virtualChestDto = virtualChestDto;
            _audioModel = _audioModelStorage.Get("UIAudio");
            
            _inputController.Lock();
            var animationAsset = Resources.Load<SkeletonDataAsset>("Sprites/Others/Chest_0/chest1_SkeletonData");
            var window = _uiService.Show<UIChest>();
            window.ChangeSkeletonAsset(animationAsset);
            window.SpinePlaceHolder.AnimationState.SetAnimation(0, _idleAnimKey, true);
            window.ActionButtonEvent += OpenChest;
            window.ChangeHeaderLabel(string.Format(_headerTextKey, "0"));
            window.ChangeButtonLabel(_openTextKey);
            window.ChangeButtonActivity(true);
        }

        private void OpenChest(object sender, EventArgs e)
        {
            var window = _uiService.Get<UIChest>();
            window.ActionButtonEvent -= OpenChest;
            window.ChangeButtonActivity(false);
            window.SpinePlaceHolder.AnimationState.SetAnimation(0, _openingAnimKey, false);
            window.SpinePlaceHolder.AnimationState.Complete += OnChestOpened;
            _soundSystem.Play(_audioModel.GetAudioClip("ChestOpen"));
            // window.
        }

        private void OnChestOpened(TrackEntry trackEntry)
        {
            var window = _uiService.Get<UIChest>();
            window.ChangeButtonLabel(_getTextKey);
            window.ChangeButtonActivity(true);
            window.ActionButtonEvent += GiveGoldToPlayer;
            window.SpinePlaceHolder.AnimationState.Complete -= OnChestOpened;
            window.SpinePlaceHolder.AnimationState.SetAnimation(0, _openedAnimKey, true);
            FillItems();
        }
        
        
        private void GiveGoldToPlayer(object sender, EventArgs eventArgs)
        {
            var window = _uiService.Get<UIChest>();
            window.ActionButtonEvent -= GiveGoldToPlayer;
            CollectReward();
            _virtualChestDto.IsOpened = true;
        }

        private void FillItems()
        {
            var window = _uiService.Get<UIChest>();
            var item = _instantiator.InstantiatePrefabForComponent<CurrencyItem>
            (window.CurrencyItemPrfab,
                window.ContentContainer);
            var settingModel = _currencySettingStorage.Get(_virtualChestDto.CurrencyID);
            item.SetText(_virtualChestDto.Reward.ToString());
            item.SetSprite(_iconLoader.LoadCurrency(_virtualChestDto.CurrencyID));
            item.SetTextColor(settingModel.ConvertedColor());
            item.SetID(_virtualChestDto.CurrencyID);
            item.transform.SetAsFirstSibling();
            _items.Add(item);
        }

        private void CollectReward()
        {
            var window = _uiService.Get<UIChest>();
            int hasGold = _virtualChestDto.Reward;
            _currencyCollectorSystem.Collect(
                _farmCameraSceneObject.Camera.ScreenToWorldPoint(window.ContentContainer.position), 
                _virtualChestDto.CurrencyID,
                _virtualChestDto.Reward,
                true,
                left =>
                {
                    var collected = hasGold - left;
                    hasGold = left;
                    _user.AddCurrency(_virtualChestDto.CurrencyID, collected);
                },
                () =>
                {
                    _user.AddCurrency(_virtualChestDto.CurrencyID, hasGold);
                    Dispose();
                });
            window.ChangeButtonActivity(false);
        }
        
        private void Dispose()
        {
            foreach (var currencyItem in _items)
            {
                Object.Destroy(currencyItem.gameObject);
            }
            
            _items.Clear();
            _uiService.Hide<UIChest>();
            _inputController.UnLock();
        }
    }
}