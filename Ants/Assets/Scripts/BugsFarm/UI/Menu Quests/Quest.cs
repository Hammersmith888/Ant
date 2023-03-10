using System;
using Zenject;

public enum QuestID
{
    None,

    BuildGoldmine,
    Buy2Workers,
    Buy2Pikemen,
    PlantFlower,
    FirstFight,
    Buy2Archers,
    Win10,
    BuildArrowTarget,
    UpgradePikeman3,
    UpgradeArcher3,
    WorkerToSoldier,
    Friendship,
    Recycle500,
    Feed5000,
    BuySnail,
    BuySpider,
}


public enum QuestStatus
{
    InProcess,
    RewardNotTaken,
    RewardTaken,
}


[Serializable]
public class Quest
{
    public bool Completed => Status == QuestStatus.RewardNotTaken || Status == QuestStatus.RewardTaken;

    public QuestStatus Status { get; private set; }
    public int CompletedStages { get; private set; }
    public bool IsNew { get; private set; }
    public QuestID ID => _id;
    
    private QuestID _id;
    [NonSerialized] private Data_Other _dataOther;

    [Inject]
    public void Inject(Data_Other dataOther)
    {
        _dataOther = dataOther;
    }
    public Quest(QuestID id, Data_Other dataOther)
    {
        _id = id;
        IsNew = true;
    }
    public void SetOld()
    {
        IsNew = false;
    }
    public void StageCompleted()
    {
        SetCompletedStages(CompletedStages + 1);
    }
    public void SetCompletedStages(int stages)
    {
        if (Status != QuestStatus.InProcess)
            return;

        int totalStages = _dataOther.Other.Quests[_id].stages;

        CompletedStages = stages;

        if (CompletedStages >= totalStages)
            Status = QuestStatus.RewardNotTaken;
    }
    public void RewardTaken()
    {
        Status = QuestStatus.RewardTaken;
    }
}

