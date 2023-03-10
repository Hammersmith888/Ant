//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleContext {

    public BattleEntity lastRoomIndexEntity { get { return GetGroup(BattleMatcher.LastRoomIndex).GetSingleEntity(); } }
    public Ecs.Sources.Battle.Components.LastRoomIndexComponent lastRoomIndex { get { return lastRoomIndexEntity.lastRoomIndex; } }
    public bool hasLastRoomIndex { get { return lastRoomIndexEntity != null; } }

    public BattleEntity SetLastRoomIndex(int newValue) {
        if (hasLastRoomIndex) {
            throw new Entitas.EntitasException("Could not set LastRoomIndex!\n" + this + " already has an entity with Ecs.Sources.Battle.Components.LastRoomIndexComponent!",
                "You should check if the context already has a lastRoomIndexEntity before setting it or use context.ReplaceLastRoomIndex().");
        }
        var entity = CreateEntity();
        entity.AddLastRoomIndex(newValue);
        return entity;
    }

    public void ReplaceLastRoomIndex(int newValue) {
        var entity = lastRoomIndexEntity;
        if (entity == null) {
            entity = SetLastRoomIndex(newValue);
        } else {
            entity.ReplaceLastRoomIndex(newValue);
        }
    }

    public void RemoveLastRoomIndex() {
        lastRoomIndexEntity.Destroy();
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleEntity {

    public Ecs.Sources.Battle.Components.LastRoomIndexComponent lastRoomIndex { get { return (Ecs.Sources.Battle.Components.LastRoomIndexComponent)GetComponent(BattleComponentsLookup.LastRoomIndex); } }
    public bool hasLastRoomIndex { get { return HasComponent(BattleComponentsLookup.LastRoomIndex); } }

    public void AddLastRoomIndex(int newValue) {
        var index = BattleComponentsLookup.LastRoomIndex;
        var component = CreateComponent<Ecs.Sources.Battle.Components.LastRoomIndexComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceLastRoomIndex(int newValue) {
        var index = BattleComponentsLookup.LastRoomIndex;
        var component = CreateComponent<Ecs.Sources.Battle.Components.LastRoomIndexComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveLastRoomIndex() {
        RemoveComponent(BattleComponentsLookup.LastRoomIndex);
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentMatcherApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class BattleMatcher {

    static Entitas.IMatcher<BattleEntity> _matcherLastRoomIndex;

    public static Entitas.IMatcher<BattleEntity> LastRoomIndex {
        get {
            if (_matcherLastRoomIndex == null) {
                var matcher = (Entitas.Matcher<BattleEntity>)Entitas.Matcher<BattleEntity>.AllOf(BattleComponentsLookup.LastRoomIndex);
                matcher.componentNames = BattleComponentsLookup.componentNames;
                _matcherLastRoomIndex = matcher;
            }

            return _matcherLastRoomIndex;
        }
    }
}