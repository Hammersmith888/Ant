using System;
using UnityEngine;

public enum FoodType
{
    None,

    SunflowerSeeds,
    Wheat,
    Seeds,
    Raspberry,
    Grapes,
    Huzelnut,
    Apple,

    FoodStock,
    FightStock,
    Garden,
    DumpsterStock,
    PileStock,

    Buckwheat,
    Burger,
    Cabbage,
    Chocolate,
    Corn,
    Mushroom,
    Potato,
    Sugar,
    Meat
}

[Serializable]
public struct FoodStage
{
    public float quantity;
    public Sprite sprite;

    public void SetQuantity(float quantity) => this.quantity = quantity;
}

[CreateAssetMenu(
                    fileName = ScrObjs.CfgFood,
                    menuName = ScrObjs.folder + ScrObjs.CfgFood,
                    order = ScrObjs.CfgFood_i
                )]
public class CfgFood : CfgObject
{
    public FoodType foodType;

    public bool isRestockable;

    public FoodStage[] stages;

    public int garbageAmount;
}