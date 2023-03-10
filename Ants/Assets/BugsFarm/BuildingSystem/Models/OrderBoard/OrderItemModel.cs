using System;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class OrderItemModel
    {
        public bool IsUnit;
        public string ItemId;
        public string ModelID;
        public string LocalizationID;
        public OrderItemParam[] Params;
        public OrderItemParam[] Rewards;
    }
}