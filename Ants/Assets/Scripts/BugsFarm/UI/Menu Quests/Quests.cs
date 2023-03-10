using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;

public static class Quests
{
    public static void Init()
    {
        GameEvents.OnAntTypeBought += OnAntTypeBought;
        GameEvents.OnObjectBought += OnObjectBought;
        GameEvents.OnObjectLevel1BuildComplete += OnObjectLevel1BuildComplete;
    }
    public static void cb_FoodEaten(float amount)
    {
        SetCompletedStages(QuestID.Feed5000, Mathf.RoundToInt(amount));
    }
    public static void cb_GarbageRecycled(float amount)
    {
        SetCompletedStages(QuestID.Recycle500, Mathf.RoundToInt(amount));
    }
    public static void cb_Worker2Soldier()
    {
        StageCompleted(QuestID.WorkerToSoldier);
    }
    
    private static void OnObjectLevel1BuildComplete(APlaceable placeable)
    {
        switch (placeable.Type)
        {
            case ObjType.str_Goldmine:
                StageCompleted(QuestID.BuildGoldmine);
                break;
            case ObjType.str_ArrowTarget:
                StageCompleted(QuestID.BuildArrowTarget);
                break;
        }
    }
    private static void OnObjectBought(APlaceable placeable)
    {
        switch (placeable.Type)
        {
            case ObjType.Decoration:
                switch ((DecorType) placeable.SubType)
                {
                    case DecorType.Flower:
                        StageCompleted(QuestID.PlantFlower);
                        break;
                }

                break;
        }
    }
    private static void OnAntTypeBought(AntType type)
    {
        switch (type)
        {
            case AntType.Worker:
                StageCompleted(QuestID.Buy2Workers);
                break;
            case AntType.Pikeman:
                StageCompleted(QuestID.Buy2Pikemen);
                break;
            case AntType.Archer:
                StageCompleted(QuestID.Buy2Archers);
                break;
            case AntType.Snail:
                StageCompleted(QuestID.BuySnail);
                break;
            case AntType.Spider:
                StageCompleted(QuestID.BuySpider);
                break;
        }
    }
    private static void StageCompleted(QuestID id)
    {
        Keeper.Quests[id].StageCompleted();

        //if (SimulationOld.Type == SimulationType.None)
            //menu_Quests.Instance.Refresh();
    }
    private static void SetCompletedStages(QuestID id, int stages)
    {
        Keeper.Quests[id].SetCompletedStages(stages);

        //if (SimulationOld.Type == SimulationType.None)
            //menu_Quests.Instance.Refresh();
    }
}