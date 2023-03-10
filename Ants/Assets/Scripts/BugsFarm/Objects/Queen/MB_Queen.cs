using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using BugsFarm.SpeakerSystem;
using Spine.Unity;
using UnityEngine;

public class MB_Queen : A_MB_Placeable_T<QueenPressenter>
{
    public override string ResourceAmount => $"личинок: {Keeper.Maggots.Count}";
    public override bool ShowCoin => false;
    public IPosSide Point => _gSide;
    public SkeletonAnimation Spine => _spine;
    //public SpeakerView SpeakerView => _speakerView;
    public IPosSide[] MaggotPoints => _maggotPoints.ToArray();
    
    [SerializeField] private SkeletonAnimation _spine;
    [SerializeField] private MB_PosSide _gSide;
    [SerializeField] private List<MB_PosSide> _maggotPoints;
    //[SerializeField] private SpeakerView _speakerView;
    
    protected override void OnSetPlacePos()
    {

    }
}