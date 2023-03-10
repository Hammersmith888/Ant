using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class RoomBuyInteractor : InteractionBaseCommand
    {
        private readonly IconLoader _iconLoader;
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IUIService _uiService;
        private readonly RoomDtoStorage _dtoStorage;
        private readonly CurrencySettingStorage _currencySettingStorage;
        private readonly RoomModelStorage _modelStorage;
        private readonly List<CurrencyItem> _items;
        
        private const string _currencyKey = "Currency_";
        private const string _currencyFreeKey = "Free";
        private string _roomGuid;
        
        public RoomBuyInteractor(IconLoader iconLoader,
                                 IInstantiator instantiator,
                                 IInputController<MainLayer> inputController,
                                 IUIService uiService, 
                                 RoomDtoStorage dtoStorage,
                                 CurrencySettingStorage currencySettingStorage,
                                 RoomModelStorage modelStorage)
        {
            _iconLoader = iconLoader;
            _instantiator = instantiator;
            _inputController = inputController;
            _uiService = uiService;
            _dtoStorage = dtoStorage;
            _currencySettingStorage = currencySettingStorage;
            _modelStorage = modelStorage;
            _items = new List<CurrencyItem>();
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            // lock all input world interactions
            _inputController.Lock();
            Setup(protocol.Guid);

            return Task.CompletedTask;
        }
        
        public void Setup(string roomGuid)
        {
            _roomGuid = roomGuid;
            
            if (!_dtoStorage.HasEntity(roomGuid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Setup)} :: {nameof(RoomDto)} with [Guid : {roomGuid}] does not exist.");
            }
            var dto = _dtoStorage.Get(roomGuid);
            if (!_modelStorage.HasEntity(dto.ModelID))
            {
                throw new InvalidOperationException($"{this} : {nameof(Setup)} :: {nameof(RoomModel)} with [ModelID : {dto.ModelID}] does not exist.");
            }
            
            var model = _modelStorage.Get(dto.ModelID);
            var localizedHeader = LocalizationManager.Localize(model.TypeName);
            var window = _uiService.Show<UIRoomBuyWindow>();
            window.CloseEvent += OnClose;
            window.NoEvent += OnClose;
            window.YesEvent += OnYes;
            window.SetHeader(localizedHeader);
            if (model.Price == null || model.Price.Length == 0)
            {
                window.SetDefaultItem(true);
                window.SetDefaultItemText(LocalizationManager.Localize(_currencyFreeKey));
                return;
            }

            foreach (var currencyModel in model.Price)
            {
                var icon = _iconLoader.Load(_currencyKey + currencyModel.ModelID);
                var item = _instantiator.InstantiatePrefabForComponent<CurrencyItem>(window.ItemPrefab, window.ItemContainer);
                var settingModel = _currencySettingStorage.Get(currencyModel.ModelID);
                item.SetText(currencyModel.Count.ToString());
                item.SetTextColor(settingModel.ConvertedColor());
                item.SetSprite(icon);
                _items.Add(item);
            }
        }
        
        private void FinalizeInteractor()
        {
            var window = _uiService.Get<UIRoomBuyWindow>();
            window.SetDefaultItem(false);
            _uiService.Hide<UIRoomBuyWindow>();
            
            _inputController.UnLock();
            foreach (var currencyItem in _items)
            {
                Object.Destroy(currencyItem.gameObject);
            }
            
            _items.Clear();
            _roomGuid = string.Empty;
        }
        
        private void OnClose(object sender, EventArgs e)
        {
            FinalizeInteractor();
        }
        
        private void OnYes(object sender, EventArgs e)
        {
            _instantiator.Instantiate<OpenRoomCommand>().Execute(new OpenRoomProtocol{Guid = _roomGuid});
            FinalizeInteractor();
        }
    }
}