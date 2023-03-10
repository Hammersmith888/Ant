using System.Collections.Generic;
using System.Linq;
using BugsFarm;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.BuildingSystem.Obsolete;
using BugsFarm.Game;
using BugsFarm.Objects.Stock.Utils;
using UnityEngine;
using UnityEngine.UI;

public enum TutorialStage
{
    None,

    Hello,
    Queen,

    BuyWorker1_text,
    BuyWorker1_BugsButton,
    BuyWorker1_WorkerButton,
    BuyWorker1_Great,

    BuildGarden_text,
    BuildGarden_FarmButton,
    BuildGarden_BuildingsTab,
    BuildGarden_GardenButton,
    BuildGarden_PlaceGarden,
    BuildGarden_Great,

    BuyFood_text,
    BuyFood_FarmButton,
    BuyFood_FoodTab,
    BuyFood_SunflowerSeedsButton,
    BuyFood_PlaceSunflowerSeeds,
    BuyFood_Great,

    BuildGoldmine_text,
    BuildGoldmine_FarmButton,
    BuildGoldmine_BuildingsTab,
    BuildGoldmine_GoldmineButton,
    BuildGoldmine_PlaceGoldmine,
    BuildGoldmine_Great,

    BuyWorker2_text,
    BuyWorker2_BugsButton,
    BuyWorker2_WorkerButton,
    BuyWorker2_Great,

    Maggots_text1,
    Maggots_wait,
    Maggots_text2,

    BuyPikeman_text,
    BuyPikeman_BugsButton,
    BuyPikeman_PikemanButton,
    BuyPikeman_Great,

    PutFightStock_text,
    PutFightStock_FarmButton,
    PutFightStock_FightStockButton,
    PutFightStock_PlaceFightStock,
    PutFightStock_Great,

    Restock_text,
    Restock_FightStock,
    Restock_HideArrow,
    Restock_Restock,
    Restock_Great,

    Fight_AttackButton,
    Fight_DragPikeman,
    Fight_Done,
    Fight_Fight,
    Fight_ChestClosed,
    Fight_WinPanelOpenAnimDone,
    Fight_ChestOpenAnimation,
    Fight_Wait,
    Fight_ShowReward,
    Fight_GetRewardAnimation,
    Fight_ContinueOrExit,
    Fight_Great,

    OpenRoom_text1,
    OpenRoom_text2,
    OpenRoom_ArrowAtRoom,
    OpenRoom_HideArrow,
    OpenRoom_ArrowAtYesOpenRoom,
    OpenRoom_Great,

    Trainig_ArrowAtFarmButton,
    Trainig_ArrowAtPikes,
    Trainig_ArrowAtPikesPlace,
    Trainig_Great,

    FinalWords,
    Close,
}


public enum TutorialStageText
{
    Hello = 1,
    Queen = 2,
    AddAnt1 = 3,
    BuildGarden = 4,
    BuyFood = 5,
    BuildGoldmine = 7,
    AddSecondAnt = 8,
    Maggots1 = 9,
    Maggots2 = 10,
    BuyWarrior = 11,
    PutFightStock = 12,
    Restock = 13,
    OpenRoom1 = 14,
    OpenRoom2 = 15,
    FinalWords = 16,
    Close = 17,
}


public class Tutorial : MB_Singleton<Tutorial>
{
    public bool IsActive => _stage != TutorialStage.None;
    //public bool IsRestockAllowed => !IsActive || _roomTarget.IsOpened;


    Dictionary<TutorialStage, TutorialStageText> stageTexts = new Dictionary<TutorialStage, TutorialStageText>
    {
        [TutorialStage.Hello] = TutorialStageText.Hello,
        [TutorialStage.Queen] = TutorialStageText.Queen,
        [TutorialStage.BuyWorker1_text] = TutorialStageText.AddAnt1,
        [TutorialStage.BuildGarden_text] = TutorialStageText.BuildGarden,
        [TutorialStage.BuyFood_text] = TutorialStageText.BuyFood,
        [TutorialStage.BuildGoldmine_text] = TutorialStageText.BuildGoldmine,
        [TutorialStage.BuyWorker2_text] = TutorialStageText.AddSecondAnt,
        [TutorialStage.Maggots_text1] = TutorialStageText.Maggots1,
        [TutorialStage.Maggots_text2] = TutorialStageText.Maggots2,
        [TutorialStage.BuyPikeman_text] = TutorialStageText.BuyWarrior,
        [TutorialStage.PutFightStock_text] = TutorialStageText.PutFightStock,
        [TutorialStage.Restock_text] = TutorialStageText.Restock,
        [TutorialStage.OpenRoom_text1] = TutorialStageText.OpenRoom1,
        [TutorialStage.OpenRoom_text2] = TutorialStageText.OpenRoom2,
        [TutorialStage.FinalWords] = TutorialStageText.FinalWords,
    };


#pragma warning disable 0649

    [SerializeField] TutorialTexts _texts;
    [SerializeField] GameObject _tapScreen;
    [SerializeField] GameObject _farmBlockRaycast;

#pragma warning restore 0649


    TutorialStage _stage;
    //RoomBase _roomTarget;

    public void StartTutor()
    {
        // if(!GameInit.FarmServices.Rooms.TryGet(1, out _roomTarget)) // получаем ссылку на комнату с ID : 1 , эта комната бесплатная;
        // {
        //     Debug.LogError($"{this} : Не удалось получить комнату для обучения!!!");
        // }
        Transition(TutorialStage.Hello);
    }


    public void SkipTutor()
    {
        Transition(TutorialStage.None);
        Analytics.TutorialSkip();
    }


    public void Transition(TutorialStage stage)
    {
        _stage = stage;

        // Principle: block all, unblock what's required
        if (stage != TutorialStage.None)
            SetBlock(true);

        Transition_StageSpecific(stage);

        if (stageTexts.TryGetValue(stage, out TutorialStageText text))
            Guide.Instance.SetText(_texts.texts[text]);
        else
            Guide.Instance.HideTextPanel();

        // Analytics
        switch (stage)
        {
            case TutorialStage.Hello: Analytics.TutorialStart(); break;
            default: Analytics.TutorialStep((int)stage - 1); break;
            case TutorialStage.Close: Analytics.TutorialComplete(); break;
            case TutorialStage.None:        /* Do nothing */
                                                                break;
        }
    }


    void Transition_StageSpecific(TutorialStage stage)
    {
        switch (stage)
        {
            case TutorialStage.Hello:
                OpenCloseTutor(true);
                Guide.Instance._button_TutorialSkip.SetActive(true);
                break;

            case TutorialStage.Queen:
                // Restore state from actions on previous stage(s)
                Guide.Instance._button_TutorialSkip.SetActive(false);
                break;

            #region Buy Worker 1

            case TutorialStage.BuyWorker1_BugsButton:
                AskPlayerToOpenPanel(PanelID.MenuBugs);
                break;

            case TutorialStage.BuyWorker1_WorkerButton:
                AskPlayerToBuy(AntType.Worker);
                break;

            case TutorialStage.BuyWorker1_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                //UI_Control.Instance.Close(PanelID.MenuBugs);

                Guide.Instance.Great();
                break;

            #endregion
            #region Build Garden

            case TutorialStage.BuildGarden_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.BuildGarden_FarmButton:
                AskPlayerToOpenPanel(PanelID.MenuFarm);
                break;

            case TutorialStage.BuildGarden_BuildingsTab:
                AskPlayerToPressButton(Panel_FarmMenu.Instance._button_Structures);
                break;

            case TutorialStage.BuildGarden_GardenButton:
                // Restore state from actions on previous stage(s)
                Guide.Instance.GuideEnabled = true;

                AskPlayerToBuy(ObjType.Food, (int)FoodType.Garden);
                break;
            case TutorialStage.BuildGarden_PlaceGarden:
                TutorArrow.Instance.PointTo(ObjType.Food, (int)FoodType.Garden, 8);
                break;

            case TutorialStage.BuildGarden_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();

                Guide.Instance.Great();
                break;

            #endregion
            #region Buy Food

            case TutorialStage.BuyFood_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.BuyFood_FarmButton:
                AskPlayerToOpenPanel(PanelID.MenuFarm);
                break;

            case TutorialStage.BuyFood_FoodTab:
                AskPlayerToPressButton(Panel_FarmMenu.Instance._button_Food);
                break;

            case TutorialStage.BuyFood_SunflowerSeedsButton:
                // Restore state from actions on previous stage(s)
                Guide.Instance.GuideEnabled = true;

                AskPlayerToBuy(ObjType.Food, (int)FoodType.SunflowerSeeds);
                break;

            case TutorialStage.BuyFood_PlaceSunflowerSeeds:
                AskPlayerToSelectPlace(
                    ObjType.Food, (int)FoodType.SunflowerSeeds,
                    Constants.TutorSunflowerSeedsPlace,
                    true
                );
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.BuyFood_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                _tapScreen.SetActive(false);
                //UI_Control.Instance.Close(PanelID.PopupSelectPlace);

                //MonoFactory.Instance.Spawn(Constants.TutorSunflowerSeedsPlace, ObjType.Food, (int)FoodType.SunflowerSeeds);
                Sounds.Play(Sound.PutFoodOrPlant);
                Guide.Instance.Great();
                break;

            #endregion
            #region Goldmine

            case TutorialStage.BuildGoldmine_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.BuildGoldmine_FarmButton:
                AskPlayerToOpenPanel(PanelID.MenuFarm);
                break;

            case TutorialStage.BuildGoldmine_BuildingsTab:
                AskPlayerToPressButton(Panel_FarmMenu.Instance._button_Structures);
                break;

            case TutorialStage.BuildGoldmine_GoldmineButton:
                // Restore state from actions on previous stage(s)
                Guide.Instance.GuideEnabled = true;

                AskPlayerToBuy(ObjType.str_Goldmine);
                break;

            case TutorialStage.BuildGoldmine_PlaceGoldmine:
                AskPlayerToSelectPlace(
                    ObjType.str_Goldmine, 0,
                    Constants.TutorGoldminePlace
                );
                break;

            case TutorialStage.BuildGoldmine_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();

                Guide.Instance.Great();
                break;

            #endregion
            #region Worker 2

            case TutorialStage.BuyWorker2_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.BuyWorker2_BugsButton:
                AskPlayerToOpenPanel(PanelID.MenuBugs);
                break;

            case TutorialStage.BuyWorker2_WorkerButton:
                AskPlayerToBuy(AntType.Worker);
                break;

            case TutorialStage.BuyWorker2_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                //UI_Control.Instance.Close(PanelID.MenuBugs);

                Guide.Instance.Great();
                break;

            #endregion
            #region Maggots

            case TutorialStage.Maggots_text1:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.Maggots_wait:
                Guide.Instance.HideTextPanel();
                ((QueenPressenter)Keeper.GetObjects(ObjType.Queen).First()).GiveBirth();
                _tapScreen.SetActive(false);
                break;

            case TutorialStage.Maggots_text2:
                _tapScreen.SetActive(true);
                break;

            #endregion
            #region Pikeman

            case TutorialStage.BuyPikeman_text:
                break;

            case TutorialStage.BuyPikeman_BugsButton:
                AskPlayerToOpenPanel(PanelID.MenuBugs);
                break;

            case TutorialStage.BuyPikeman_PikemanButton:
                AskPlayerToBuy(AntType.Pikeman);
                break;

            case TutorialStage.BuyPikeman_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                //UI_Control.Instance.Close(PanelID.MenuBugs);

                Guide.Instance.Great();
                break;

            #endregion
            #region Fight Stock

            case TutorialStage.PutFightStock_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.PutFightStock_FarmButton:
                AskPlayerToOpenPanel(PanelID.MenuFarm);
                break;

            case TutorialStage.PutFightStock_FightStockButton:
                // Restore state from actions on previous stage(s)
                Guide.Instance.GuideEnabled = true;

                AskPlayerToBuy(ObjType.Food, (int)FoodType.FightStock);
                break;

            case TutorialStage.PutFightStock_PlaceFightStock:
                AskPlayerToSelectPlace(
                    ObjType.Food, (int)FoodType.FightStock,
                    Constants.TutorFightStockPlace
                );
                break;

            case TutorialStage.PutFightStock_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();

                Guide.Instance.Great();
                break;

            #endregion
            #region Restock

            case TutorialStage.Restock_text:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.Restock_FightStock:
                _tapScreen.SetActive(false);
                _farmBlockRaycast.SetActive(false);

                var stock = Stock.Find<FightStockPresenter>().FirstOrDefault();
                stock.SetColliderAllowed(true);
                TutorArrow.Instance.PointTo(stock.MB_Food.transform);
                break;

            case TutorialStage.Restock_HideArrow:
                TutorArrow.Instance.Hide();
                break;

            case TutorialStage.Restock_Restock:
                //InfoPanelButton.Instance._button_Upgrade.enabled = true; TODO: Туториал Инфо панель
                //TutorArrow.Instance.PointTo(InfoPanelButton.Instance._button_Upgrade.transform); TODO: Туториал Инфо панель
                break;

            case TutorialStage.Restock_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                //UI_Control.Instance.Close(PanelID.HudInfoPanel);

                Guide.Instance.Great();
                break;

            #endregion
            #region Fight

            case TutorialStage.Fight_AttackButton:
                AskPlayerToOpenPanel(PanelID.SquadSelect);
                break;

            case TutorialStage.Fight_DragPikeman:
                /*Tools.Log();
                Assert.IsNotNull(UiSquadSelectView.Instance, "SquadSelect.Instance");
                foreach (SquadSelectItem item in UiSquadSelectView.Instance.Items)
                {
                    Tools.Log();
                    Assert.IsNotNull(item.Ant, "item.Ant");
                    if (item.Ant.Type != AntType.Pikeman)
                        item.RaycastTarget = false;
                    Tools.Log();
                }

                Tools.Log();
                Assert.IsNotNull(TutorArrow.Instance, "TutorArrow.Instance");
                Assert.IsNotNull(UiSquadSelectView.Instance.Items, "SquadSelect.Instance.Items");
                Assert.IsNotNull(UiSquadSelectView.Instance.Slots, "SquadSelect.Instance._slots");
                Assert.IsNotNull(UiSquadSelectView.Instance.Items.Find(x => x.Ant.Type == AntType.Pikeman), "SquadSelect.Instance.Items.Find( x => x.Ant.Type == AntType.Pikeman )");
                Assert.IsNotNull(UiSquadSelectView.Instance.Slots[0], "SquadSelect.Instance._slots[ 0 ]");
                TutorArrow.Instance.TweenDrag(
                    UiSquadSelectView.Instance.Items.Find(x => x.Ant.Type == AntType.Pikeman).transform.position,
                    UiSquadSelectView.Instance.Slots[0].transform.position
                );
                Tools.Log();*/
                break;

            case TutorialStage.Fight_Done:
                /*UiSquadSelectView.Instance.ButtonGoFight.enabled = true;
                foreach (SquadSelectItem item in UiSquadSelectView.Instance.Items)
                    item.RaycastTarget = false;
                TutorArrow.Instance.PointTo(UiSquadSelectView.Instance.ButtonGoFight.transform);*/
                break;

            case TutorialStage.Fight_Fight:
                TutorArrow.Instance.Hide();
                Guide.Instance.GuideEnabled = false;
                break;

            case TutorialStage.Fight_ChestClosed:                       // WinPanelOpenAnimStart
                break;

            case TutorialStage.Fight_WinPanelOpenAnimDone:              // WinPanelOpenAnimDone
                                                                        // TutorArrow.Instance.PointTo( WinPanel.Instance._buttonOpenGet.transform );
                break;

            case TutorialStage.Fight_ChestOpenAnimation:
                TutorArrow.Instance.Hide();
                break;

            case TutorialStage.Fight_Wait:
                break;

            case TutorialStage.Fight_ShowReward:
                // TutorArrow.Instance.PointTo( WinPanel.Instance._buttonOpenGet.transform );
                break;

            case TutorialStage.Fight_GetRewardAnimation:
                TutorArrow.Instance.Hide();
                break;

            case TutorialStage.Fight_ContinueOrExit:
                //TutorArrow.Instance.PointTo(WinPanel.Instance._buttonReturn.transform, false, true);
                break;

            case TutorialStage.Fight_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();
                Guide.Instance.GuideEnabled = true;

                Guide.Instance.Great();
                break;

            #endregion
            #region Open Room

            case TutorialStage.OpenRoom_text1:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.OpenRoom_ArrowAtRoom:
                _tapScreen.SetActive(false);
                _farmBlockRaycast.SetActive(false);
                //TutorArrow.Instance.PointTo(_roomTarget.Position);
                break;

            case TutorialStage.OpenRoom_HideArrow:
                TutorArrow.Instance.Hide();
                break;

            case TutorialStage.OpenRoom_ArrowAtYesOpenRoom:
                TutorArrow.Instance.PointTo(Panel_YesNo.Instance._button_Yes.transform, true);
                break;

            case TutorialStage.OpenRoom_Great:
                // Restore state from actions on previous stage(s)
                TutorArrow.Instance.Hide();

                Guide.Instance.Great();
                break;

            #endregion
            #region Trainig

            case TutorialStage.Trainig_ArrowAtFarmButton:
                AskPlayerToOpenPanel(PanelID.MenuFarm);
                break;

            case TutorialStage.Trainig_ArrowAtPikes:
                // Restore state from actions on previous stage(s)
                Guide.Instance.GuideEnabled = true;

                AskPlayerToBuy(ObjType.str_Pikes);
                break;

            case TutorialStage.Trainig_ArrowAtPikesPlace:
                //RoomsBook.Get(1).ChestColliderEnabled = false;
                Debug.LogError($"{this} : Нужно добавить выключение тригера сундука");
                AskPlayerToSelectPlace(ObjType.str_Pikes, 0, 12);
                break;

            case TutorialStage.Trainig_Great:
                // Restore state from actions on previous stage(s)
                //RoomsBook.Get(1).ChestColliderEnabled = true;
                Debug.LogError($"{this} : Нужно добавить включение тригера сундука");
                TutorArrow.Instance.Hide();

                Guide.Instance.Great();
                break;

            #endregion

            case TutorialStage.FinalWords:
                // Restore state from actions on previous stage(s)
                _tapScreen.SetActive(true);
                break;

            case TutorialStage.Close:
                Transition(TutorialStage.None);
                break;

            case TutorialStage.None:
                SetBlock(false);
                OpenCloseTutor(false);
                break;
        }
    }


    void OpenCloseTutor(bool open)
    {
        //UI_Control.Instance.OpenClose(PanelID.Guide, open);
        SetSubscription(open);
    }


    void AskPlayerToBuy(ObjType type, int subType = 0)
    {
        Panel_FarmMenu.Instance.SetBlock(type, subType, false, true);
        TutorArrow.Instance.PointTo(Panel_FarmMenu.Instance.GetButtonPos(type, subType));
    }


    void AskPlayerToBuy(AntType type)
    {
        //UIBugShop.Instance.SetBlock(type, false);
        //TutorArrow.Instance.PointTo(UIBugShop.Instance.GetButtonPos(type), false, true);
    }


    void AskPlayerToOpenPanel(PanelID panel_id)
    {
        AskPlayerToPressButton(HUD_Bottom.Instance.Buttons[panel_id], false);
        _tapScreen.SetActive(false);

        if (panel_id == PanelID.MenuFarm)
            Guide.Instance.GuideEnabled = false;
    }


    void AskPlayerToPressButton(Button button, bool flipY = true)
    {
        button.enabled = true;
        TutorArrow.Instance.PointTo(button.transform.position, true, flipY);
    }


    void AskPlayerToSelectPlace(ObjType type, int subType, int placeNum, bool blockAll = false)
    {
        var places = PlacesBook.GetPlaces(type, subType);

        //foreach (var place in places)
            //if (blockAll || place.PlaceNum != placeNum)
                //place.Blocked = true;

        places[Constants.QueenPlace].gameObject.SetActive(false);

        TutorArrow.Instance.PointTo(
            places[placeNum].transform.position +
            new Vector3(.25f, -1, 0)
        );
    }
    void SetBlock(bool isBlocked)
    {
        bool isEnabled = !isBlocked;

        _farmBlockRaycast.SetActive(isBlocked);

        //UI_Control.Instance.InvisibleButtonEnabled = isEnabled;

        // Bowl
        //Keeper.Bowl.MB_Bowl.WorldCanvasButton.enabled = isEnabled; TODO: Туториал поилка должна блокироваться

        // HUD Top
        //UI_Control.Instance.HUDTop.SetBlock(isEnabled);

        // HUD Bottom
        HUD_Bottom.Instance.SetBlock(isBlocked);

        //UIBugShop.Instance.SetBlock(isBlocked);
        Panel_FarmMenu.Instance.SetBlock(isBlocked);
        //UI_Control.Instance.HUDShopItemInfo.ShopItemInfoView.SetBlock(isBlocked);

        // Info Panel TODO: Туториал Инфо панель
        // InfoPanel.Instance._button_Remove.enabled = isEnabled; TODO: Туториал Инфо панель
        // InfoPanel.Instance._button_Replace.enabled = isEnabled; TODO: Туториал Инфо панель
        // InfoPanelButton.Instance._button_Upgrade.enabled = isEnabled; TODO: Туториал Инфо панель
        // InfoPanelButton.Instance._button_Close.enabled = isEnabled; TODO: Туториал Инфо панель

        // Yes/No panel 
        Panel_YesNo.Instance._button_No.enabled = isEnabled;
        Panel_YesNo.Instance._button_Close.enabled = isEnabled;

        // Squad Select
        // SquadSelect.Instance._buttonGoFight.enabled = isEnabled; TODO: Туториал выбор отряда
        // SquadSelect.Instance._buttonPlus.enabled = isEnabled; TODO: Туториал выбор отряда
        // SquadSelect.Instance._buttonClose.enabled = isEnabled; TODO: Туториал выбор отряда
        //
        // WinPanel.Instance._buttonNextRoom.enabled = isEnabled; TODO: Туториал панель победителя в боёвке

        Panel_SelectPlace.Instance.buttonCancel.SetActive(isEnabled);

        // GamePlay
        foreach (var pair in Keeper.Objects)
        {
            foreach (var placeable in pair.Value)
            {
                if(placeable.Type == ObjType.DigGroundStock) continue;
                if(placeable.Type == ObjType.Food && placeable.SubType == (int)FoodType.PileStock) continue;
                placeable.SetColliderAllowed(isEnabled);
            }
        }

        Keeper.Ants.ForEach(x => x.SetCollidersAllowed(isEnabled));
    }


    public void NextStage() => Transition(_stage + 1);


    void SetSubscription(bool subscribe)
    {
        if (subscribe)
        {
            GameEvents.OnTutorialGreatAnimDone += NextStage;
            GameEvents.PanelOpenAnimDone += NextStage;
            GameEvents.TabOpened += NextStage;
            GameEvents.OnMaggotSpawned += NextStage;
            GameEvents.OnUnitSelected += NextStage;
            GameEvents.OnWinPanelNextStage += OnWinPanelNextStage;
            GameResources.OnCoinsChanged += OnCoinsChanged;
        }
        else
        {
            GameEvents.OnTutorialGreatAnimDone -= NextStage;
            GameEvents.PanelOpenAnimDone -= NextStage;
            GameEvents.TabOpened -= NextStage;
            GameEvents.OnMaggotSpawned -= NextStage;
            GameEvents.OnUnitSelected -= NextStage;
            GameEvents.OnWinPanelNextStage -= OnWinPanelNextStage;
            GameResources.OnCoinsChanged -= OnCoinsChanged;
        }
    }


    void OnWinPanelNextStage(WinPanelStage stage) => NextStage();


    void OnCoinsChanged()
    {
        if (
                _stage < TutorialStage.Fight_GetRewardAnimation ||
                _stage >= TutorialStage.Trainig_ArrowAtPikesPlace
            )
            NextStage();
    }


    void OnPanelOpened(object sender, PanelID panel_id)
    {
        if (
                panel_id != PanelID.FightWin
            )
            NextStage();
    }


    void OnPanelClosed(object sender, PanelID panel_id)
    {
        if (
                panel_id == PanelID.FightWin ||
                panel_id == PanelID.PopupYesNoIap
            )
            NextStage();
    }


    public bool IsRoomTapAllowed(int roomNum)
    {
        return _stage == TutorialStage.None || _stage == TutorialStage.OpenRoom_ArrowAtRoom && roomNum == 1 ;
    }
}

