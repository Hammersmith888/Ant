using System;
using System.Collections.Generic;
using System.Linq;

/*
	(*)
	This check can be omitted for bowl, but MUST BE CHECKED for food.
	Reason - foods, having isRestockable == TRUE - we don't want to enqueue to them
*/
public static class FindFood
{
    private static ObjType TypeFood => ObjType.Food;

    private static readonly Func<Food, bool> eatAll = (food) =>
    {
        return !food.IsGarbage &&
               food.IsReady &&          // exclude upgrading Garden
               food.FoodType != FoodType.FightStock &&
               food.QuantityCur > 0;
    };
    private static readonly Func<Food, bool> eat1 = (food) =>
    {
        return !food.IsGarbage &&
                food.FoodType != FoodType.FightStock &&
                food.FoodType != FoodType.FoodStock &&
                food.FoodType != FoodType.Garden &&
                food.QuantityCur > 0;
    };
    private static readonly Func<Food, bool> eat2 = (food) =>
    {
        return !food.IsGarbage &&
                food.IsReady &&            // exclude upgrading Garden
                food.FoodType == FoodType.Garden &&
                food.QuantityCur > 0;                              // (*)
    };
    private static readonly Func<Food, bool> eat3 = (food) =>
    {
        return !food.IsGarbage &&
                food.FoodType == FoodType.FoodStock &&
                food.QuantityCur > 0;                            // (*)
    };
    private static readonly Func<Food, bool> garbage = food =>
    {
       return food.IsGarbage && food.FoodType != FoodType.DumpsterStock;
    };
    
    private static readonly Func<GardenPresenter, bool> _gardening = food 
        => food.FoodType == FoodType.Garden && food.IsReady && !food.WasGardenedRecently;

    public static IEnumerable<APlaceable> Shuffle(IEnumerable<APlaceable> consumables)
    {
        return Tools.Shuffle_FisherYates(consumables);
    }
    public static IEnumerable<Food> ForEatAll() => Foreach(eatAll);
    public static IEnumerable<Food> ForEat1() => Foreach(eat1);
    public static IEnumerable<Food> ForEat2() => Foreach(eat2);
    public static IEnumerable<Food> ForEat3() => Foreach(eat3);
    public static IEnumerable<Food> Garbage() => Foreach(garbage);
    public static IEnumerable<GardenPresenter> Gardening() => Foreach(_gardening);
    
    public static DumpsterStockPresenter Dumpster()
    {
        return FindFoods<DumpsterStockPresenter>().FirstOrDefault(x => x.FoodType == FoodType.DumpsterStock && x.IsReady);
    }
    public static IEnumerable<T> Foreach<T>(Func<T, bool> predicate)
    {
        return FindFoods<T>().Where(predicate);
    }
    public static IEnumerable<T> FindFoods<T>()
    {
        return Keeper.GetObjects(TypeFood).OfType<T>();
    }
}

