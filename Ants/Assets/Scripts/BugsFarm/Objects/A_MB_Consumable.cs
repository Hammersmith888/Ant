using BugsFarm.BuildingSystem;
using UnityEngine;


public abstract class A_MB_Consumable : A_MB_Placeable
{
    public override string ResourceAmount => $"{ Mathf.RoundToInt(consumable.QuantityCur) } / { Mathf.RoundToInt(consumable.QuantityMax) } ед.";
    public override bool ShowCoin => false;
    public IPosSide[] Points => _gSides;


#pragma warning disable 0649

    [SerializeField] protected MB_PosSide[] _gSides;

#pragma warning restore 0649

    protected override APlaceable placeable => consumable;
    protected abstract AConsumable consumable { get; }
    protected override void OnSetPlacePos()
    {

    }
}

