using BugsFarm.Config;
using UnityEngine;

public class CfgObject : ScriptableObject
{
    public ObjType type;
    public bool isBig;

    public bool isLocked;
    public int unlocksAfter;

    public virtual int Price => upgrades ? upgrades.levels[0].price : price;
#pragma warning disable 0649
    [SerializeField] int price;
#pragma warning restore 0649

    public int maxCount;
    public Wiki wiki;

    public A_MB_Placeable prefab;

    public CfgUpgrades upgrades;

    public void SetPrice(int price)
    {
        this.price = price;
    }
}