using BugsFarm.BuildingSystem;
using UnityEngine;


public class GoldminePlace : SpinePlace
{
    public Renderer Rocks => _rocks;
    public IPosSide PointLever => _posSideLever;
    public IPosSide PointRock => _posSideRock;

#pragma warning disable 0649
    [SerializeField] private Renderer _rocks;
    [SerializeField] private MB_PosSide _posSideLever;
    [SerializeField] private MB_PosSide _posSideRock;
#pragma warning restore 0649
    
}

