using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UnitSystem.Obsolete;

public interface IStock
{
    ObjType Type { get; }
    int SubType { get; }
    float QuantityCur { get; }
    float QuantityMax { get; }
    void Restock();
    float Add(float quantity);
    float Remove(float quantity);
    void SetQuantity(float quantity);
    void SetSubscriber(ATask subscriber, bool add);
    IPosSide GetRandomPosition(IPosSide exclude = null);
    void Reset();
}