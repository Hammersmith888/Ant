using System;
using System.Collections.Generic;
using BugsFarm.Game;
using UniRx;

namespace BugsFarm.Managers
{
    public static class FreshFood
    {
        private static List<Food> _freshFoods;

        public static void Init()
        {
            _freshFoods = new List<Food>();
            GameEvents.OnObjectBought += OnFreshFood;
            GameEvents.OnFoodRestock += OnFreshFood;
        }
    
        public static void UnInit()
        {
            _freshFoods.Clear();
            GameEvents.OnObjectBought -= OnFreshFood;
            GameEvents.OnFoodRestock -= OnFreshFood;
        }

        private static void OnFreshFood(APlaceable placeable)
        {
            if (!placeable.IsNullOrDefault() && placeable is Food food )
            {
                Add(food);
            }
        }

        private static void Add(Food food)
        {
            if(_freshFoods.Contains(food)) return;

            _freshFoods.Add(food);
            Observable.Timer(TimeSpan.FromMinutes(Constants.FreshFoodTimeMins)).Subscribe(xs =>
            {
                if (!_freshFoods.Contains(food)) 
                    return;
                _freshFoods.Remove(food);
            });
        }

        public static bool TryGetFreshFoods(out IEnumerable<Food> freshFoods)
        {
            freshFoods = _freshFoods;
            return !freshFoods.IsNullOrDefault();
        }
        
    }
}