using System.Collections.Generic;
using BugsFarm;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Config;
using BugsFarm.Game;
using UnityEngine;
using UnityEngine.UI;


public class Panel_FarmMenu : APanel
{
    private readonly List<(ObjType type, int subType)> _buildingsOrder = new List<(ObjType, int)>
    {
        (ObjType.Food, (int) FoodType.Garden),
        (ObjType.str_Goldmine, 0),
        (ObjType.Food, (int) FoodType.FightStock),
        (ObjType.str_Pikes, 0),
        (ObjType.str_ArrowTarget, 0),
        (ObjType.Food, (int) FoodType.DumpsterStock),
        (ObjType.str_SleepingPod, 0),
    };
    
    public static Panel_FarmMenu Instance { get; private set; }

    [SerializeField] private ShopItem _prefab_Item;
    [SerializeField] private Transform _content_Food;
    [SerializeField] private Transform _content_Structures;
    [SerializeField] private Transform _content_Decors;
    [SerializeField] private ScrollRect _foods;
    [SerializeField] private ScrollRect _structures;
    [SerializeField] private ScrollRect _decors;

    public Button _button_Food;
    public Button _button_Structures;
    public Button _button_Decors;

    [SerializeField] private List<Button> _topButtons;

    private readonly List<ShopItem> _items = new List<ShopItem>();

    private ObjType _selectedType;
    private int _selectedSubType;
    private float _tabYSelected;
    private float _tabYHidden;


    private void Start()
    {
        _tabYSelected = _button_Food.GetComponent<RectTransform>().anchoredPosition.y;
        _tabYHidden = _button_Structures.GetComponent<RectTransform>().anchoredPosition.y;
    }

    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = true;
        manualClose = false;

        Instance = Tools.SingletonPattern(this, Instance);

        AddItems();

        GameResources.OnCoinsChanged += Refresh;
    }

    protected override void OnOpened()
    {
        //UI_Control.Instance.HUDShopItemInfo.ShopItemInfoView.OnBuyClicked += OnBuy;
        Refresh();
    }

    protected override void OnClosed()
    {
        //UI_Control.Instance.Close(PanelID.HudShopItemInfo);
        //UI_Control.Instance.HUDShopItemInfo.ShopItemInfoView.OnBuyClicked -= OnBuy;
    }

    private void AddItems()
    {
        return;
        // Add Food items
        foreach (var data in Data_Objects.Instance.Food)
        {
            if (!data.foodType.AnyOff(FoodType.FoodStock,FoodType.PileStock))
            {
                var content = data.foodType.AnyOff(FoodType.FightStock,FoodType.Garden,FoodType.DumpsterStock)
                              ? _content_Structures
                              : _content_Food;
                CreateItem( data,content,(int) data.foodType );
            }
        }

        // Add Structure items
        foreach (var data in Data_Objects.Instance.Buildings)
        {
            if (!data.type.AnyOff(ObjType.HerbsStock,ObjType.DigGroundStock,ObjType.Queen,ObjType.Bowl))
            {
                CreateItem(data, _content_Structures, 0);
            }
        }

        // Add Decor items
        foreach (var data in Data_Objects.Instance.Decors)
        {
            CreateItem(data, _content_Decors, (int) data.decorType);
        }


        // Buildings Order
        for (var i = 0; i < _buildingsOrder.Count; i++)
        {
            var (type, subType) = _buildingsOrder[i];

            _items.Find(x => x.type == type && x.subType == subType).transform.SetSiblingIndex(i);
        }
    }

    private void CreateItem(CfgObject data, Transform parent, int subType)
    {
        var item = Instantiate(_prefab_Item, parent);

        item.Setup(data, subType);

        _items.Add(item);
    }

    private void Refresh()
    {
        foreach (var item in _items)
        {
            item.Refresh();
        }
    }

    public void OnPrepareBuy(CfgObject data, int subType)
    {
        //UI_Control.Instance.Open(PanelID.HudShopItemInfo);
        //UI_Control.Instance.HUDShopItemInfo.ShopItemInfoView.Setup(data, subType);
    }

    public void OnBuy(ObjType type, int subType)
    {
        if (type == ObjType.None ||
            (type == ObjType.Food || type == ObjType.Decoration) && subType == 0)
            return;

        var places = PlacesBook.GetPlaces(type, subType);

        if (places == null)
            return;


        _selectedType = type;
        _selectedSubType = subType;

        Close();

        Panel_SelectPlace.SetPlaces(places, cb_PlaceSelected, type);
        //UI_Control.Instance.Open(PanelID.PopupSelectPlace); // (!) AFTER SetPlaces(). Required for Tutorial.
    }

    void cb_PlaceSelected(APlace place)
    {
        var data = Data_Objects.Instance.GetData(_selectedType, _selectedSubType);

        //RemoveObjects.Instance.Buy(place.PlaceNum, data);
    }

    public void Buy(int placeNum, CfgObject data)
    {
        int price = FirstHour.GetPrice(data, _selectedSubType);
        GameResources.Coins -= price;

        APlaceable placeable = null;// MonoFactory.Instance.Spawn(placeNum, _selectedType, _selectedSubType);

        Sounds.PlayPutSound(_selectedType, _selectedSubType);

        GameEvents.OnObjectBought?.Invoke(placeable);
        Analytics.InAppPurchase(placeable, price, Currency.Coins);
    }


    public void OnFoods() => OnTab(_foods, _button_Food);
    public void OnStructures() => OnTab(_structures, _button_Structures);
    public void OnDecors() => OnTab(_decors, _button_Decors);


    void OnTab(ScrollRect activeSR, Button activeButton)
    {
        _foods.gameObject.SetActive(activeSR == _foods);
        _structures.gameObject.SetActive(activeSR == _structures);
        _decors.gameObject.SetActive(activeSR == _decors);

        SetTabY(activeButton, _button_Food);
        SetTabY(activeButton, _button_Structures);
        SetTabY(activeButton, _button_Decors);

        Canvas.ForceUpdateCanvases(); // for Tutorial
        GameEvents.TabOpened?.Invoke();
    }


    void SetTabY(Button activeButton, Button button)
    {
        RectTransform rt = button.GetComponent<RectTransform>();
        float y =
                button == activeButton ? _tabYSelected : _tabYHidden
            ;
        rt.anchoredPosition = rt.anchoredPosition.SetY(y);
    }


    public void SetBlock(bool isBlocked)
    {
        bool isEnabled = !isBlocked;

        // Tabs
        _button_Decors.enabled = isEnabled;
        _button_Structures.enabled = isEnabled;
        _button_Food.enabled = isEnabled;

        _items.ForEach(x => { x.infoButton.enabled = x.buyButton.enabled = isEnabled; });
        _topButtons.ForEach(x => x.enabled = isEnabled);
    }


    public void SetBlock(ObjType type, int subType, bool isBlocked, bool isBlockedInfo = false)
    {
        var item = FindItem(type, subType);
        item.buyButton.enabled = !isBlocked;
        item.infoButton.enabled = !isBlockedInfo;
    }

    public Vector2 GetButtonPos(ObjType type, int subType) => FindItem(type, subType).buyButton.transform.position;

    public ShopItem FindItem(ObjType type, int subType) => _items.Find(x => x.type == type && x.subType == subType);
}