using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class menu_Quests : APanel
{
    //public static menu_Quests Instance { get; private set; }

    [SerializeField] private QuestItem _prefab;
    [SerializeField] private Transform _root;
    [SerializeField] private GameObject _newQuests;
    [SerializeField] private Text _newQuestsCount;
    [FormerlySerializedAs("_dayProgress_bar")] [SerializeField] private Image _dayProgressBar;
    [FormerlySerializedAs("_dayProgress_text")] [SerializeField] private Text _dayProgressText;

    private CfgOther.TQuests Quests => _dataOther.Other.Quests;
    private IEnumerable<QuestID> QuestsOrder => _dataOther.Other.QuestsOrder;
    private readonly HashSet<QuestItem> _items = new HashSet<QuestItem>();
    private IInstantiator _instantiator;
    private Data_Other _dataOther;
    [Inject]
    private void Inject(IInstantiator instantiator)
    {
        _instantiator = instantiator;
        //_dataOther = dataOther;
    }
    protected override void Init(out bool isModal, out bool manualClose)
    {
        isModal = true;
        manualClose = false;

        //Instance = Tools.SingletonPattern(this, Instance);
    }
    protected override void OnOpened()
    {
        MarkAllOld();
    }

    public void InitQuests()
    {
        SyncQuests();
        Add_UI_QuestItems();
        Refresh();
    }
    public void Refresh()
    {
        foreach (var item in _items)
        {
            item.Refresh();
        }

        RefreshDayProgress();
        CountNewQuests();
    }
    public void DeleteItem(QuestItem item)
    {
        _items.Remove(item);

        Destroy(item.gameObject);

        if (!_items.Any())
        {
            var chestData = new CfgChest() {Coins = 1000, Crystals = 0, ID = -100};
            // TODO : Remake the open UI with chest
            //GameEvents.OnRewardTaskDay?.Invoke(chestData, ChestType.Gold);
        }
    }
    
    private void SyncQuests()
    {
        // Remove deleted quests from Keeper
        foreach (var pair in Keeper.Quests.ToList())
        {
            if (!Quests.ContainsKey(pair.Key))
                Keeper.Quests.Remove(pair.Key);
        }

        // Add missing quests to Keeper
        foreach (var id in QuestsOrder)
        {
            if (!Keeper.Quests.ContainsKey(id))
                Keeper.Quests.Add(id,_instantiator.Instantiate<Quest>(new object[]{id}));
        }
    }
    private void Add_UI_QuestItems()
    {
        // Clear
        foreach (var item in _items)
        {
            Destroy(item.gameObject);
        }

        _items.Clear();


        foreach (var id in QuestsOrder)
        {
            if ( Keeper.Quests.ContainsKey(id) && Keeper.Quests[id].Status != QuestStatus.RewardTaken)
            {
                var item = _instantiator.InstantiatePrefabForComponent<QuestItem>(_prefab, _root, new object[]{Quests[id]});
                _items.Add(item);
            }
        }
    }
    private void MarkAllOld()
    {
        foreach (var pair in Keeper.Quests)
        {
            pair.Value.SetOld();
        }

        _newQuests.gameObject.SetActive(false);
    }
    private void RefreshDayProgress()
    {
        var completed = Keeper.Quests.Count(x => x.Value.Completed);
        var total = Keeper.Quests.Count;
        var progress = (float) completed / total;
        var percent = Mathf.FloorToInt(progress * 100);

        _dayProgressBar.fillAmount = progress;
        _dayProgressText.text = $"{percent.ToString()}%";
    }
    private void CountNewQuests()
    {
        var newCount = Keeper.Quests.Count(x => x.Value.IsNew);
        _newQuestsCount.text = newCount.ToString();
        _newQuests.gameObject.SetActive(newCount > 0);
    }
}