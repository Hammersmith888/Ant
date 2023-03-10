using System;
using System.Linq;
using BugsFarm.Config;

[Serializable]
public class Food : AConsumable
{
    public override string Header => IsGarbage ? Data_Objects.Instance.GetData(FoodType.PileStock).wiki.Header : base.Header;
    public FoodType FoodType => (FoodType)SubType;
    public MB_Food MB_Food => _mb_Food;
    public CfgFood Data => _data;
    public virtual bool IsGarbage => _isGarbage;

    [NonSerialized] CfgFood _data;
    [NonSerialized] MB_Food _mb_Food;

    private bool _isGarbage;
    protected virtual bool IsInvisibleWhenEmpty => _data.isRestockable;
    protected override A_MB_Consumable MbConsumable => _mb_Food;


    public Food(FoodType foodType, int placeNum, bool full = true)
        : base(ObjType.Food,
               (int)foodType,
               placeNum,
               Data_Objects.Instance.GetData(foodType).stages[0].quantity,
               full)
    { }
    public override void Init(A_MB_Placeable mb_Food)
    {
        _mb_Food = (MB_Food)mb_Food;
        _data = Data_Objects.Instance.GetData(FoodType);
        InitOccupyable();
    }
    public override void PostSpawnInit()
    {
        base.PostSpawnInit();
        SetSprite();
    }
    public override void PostLoadRestore()
    {
        base.PostLoadRestore();

        if (_isGarbage)
            _mb_Food.SetGarbage();
        else
            SetSprite();
    }
    public override void Upgrade()
    {
        base.Upgrade();

        QuantityMax = UpgradeLevelCur.param1;

        SetSprite();        // for Dumpster
    }
    public virtual void Restock()
    {
        SetToMax();
    }
    protected override void OnDepleted()
    {
        base.OnDepleted();

        // Analytics
        if (!_isGarbage)
        {
            var noFood = !FindFood.ForEatAll().Any();

            if (noFood)
            {
                BugsFarm.Analytics.NoResources(false);
            }
        }

        if (_data.isRestockable)
            return;

        if (_isGarbage)
        {
            Keeper.Destroy(this);
        }
        else
        {
            // Make it garbage

            _isGarbage = true;
            QuantityCur = _data.garbageAmount;

            _mb_Food.SetGarbage();
        }
    }
    protected override void SetSprite()
    {
        var stages = _data.stages;
        var sprite = IsInvisibleWhenEmpty ? null : stages[stages.Length - 1].sprite;
        var scale = FoodType == FoodType.DumpsterStock ? UpgradeLevelCur.param1 / stages[0].quantity : 1;

        if (QuantityCur > 0)
        {
            var stage = 0;
            var next = 1;

            while ( next < stages.Length && QuantityCur < stages[next].quantity * scale)
            {
                stage++;
                next = stage + 1;
            }

            sprite = stages[stage].sprite;
        }

        _mb_Food.SetSprite(sprite);
    }
}

