using System;
using System.Linq;
using BugsFarm.Objects.Stock.Utils;

namespace BugsFarm.UnitSystem.Obsolete
{
    [Serializable]
    public class SM_RestockFight : SM_RestockFood
    {
        protected override int RestockSubType => (int)FoodType.FightStock;
        public override string ToString() //TODO : Добавить ключ локализации
        {
            return "запасы в бой";
        }
        public SM_RestockFight(int priority, StateAnt aiType) : base(priority,aiType) { }
        protected override bool TryOccupyStock(out IStock stock)
        {
            return !IsCompleteStock(StockCheck.Full, stock = Stock.Find(RestockType,RestockSubType).FirstOrDefault());
        }
    }
}