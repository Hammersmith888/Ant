using System;

[Serializable]
    public class FoodStockPresenter : Food, IStock
    {
        public FoodStockPresenter(FoodType stockType, int placeNum, bool full = false) : base(stockType, placeNum, full){}
        public override void Restock() => Add(Constants.RestockFightStockAmount);
        public virtual float Remove(float quantity)
        {
            return base.Add(-quantity);
        }
        public new virtual void SetQuantity(float quantity) => base.SetQuantity(quantity);
        public virtual void Reset()
        {
            QuantityCur = 0;
        }
    }