using System;
using BugsFarm.Game;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using UnityEngine;

public class RateMyApp : MonoBehaviour
{
    const string PpKey_AskedToRateGame = "AskedToRateGame";

    private void Start()
    {
        GameEvents.OnWinPanelNextStage += OnWinPanelNextStage;
    }
    
    private void Update()
    {
        TimeSpan ts = TimeSpan.FromSeconds(SimulationOld.GameAge);

        if (
            SimulationOld.Type == SimulationType.None &&
            ts.Hours > 0 &&
            ts.Hours != Keeper.Stats.LastReviewProposalHours &&
            ts.Hours % 6 == 0
        )
        {
            Keeper.Stats.LastReviewProposalHours = ts.Hours;
            //GooglePlayReview.Instance.Review();
        }
    }


    void OnWinPanelNextStage(WinPanelStage stage)
    {
        //todo get from ecs
        var fightRoomIndex = 0;
        int nextLevel = fightRoomIndex + 1;
        int completedLevel = nextLevel - 1;

        if (
            stage != WinPanelStage.ContinueOrExit ||
            completedLevel != 4
            // || PlayerPrefs.GetInt( PpKey_AskedToRateGame, 0 ) == 1
        )
            return;

        // NPBinding.Utility.RateMyApp.AskForReviewNow();
        //GooglePlayReview.Instance.Review();

        // PlayerPrefs.SetInt( PpKey_AskedToRateGame, 1 );

        // NPBinding.Utility.RateMyApp.Delegate
    }
}