//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleEntity {

    public Ecs.Sources.Battle.Components.RoomIndexComponent roomIndex { get { return (Ecs.Sources.Battle.Components.RoomIndexComponent)GetComponent(BattleComponentsLookup.RoomIndex); } }
    public bool hasRoomIndex { get { return HasComponent(BattleComponentsLookup.RoomIndex); } }

    public void AddRoomIndex(int newValue) {
        var index = BattleComponentsLookup.RoomIndex;
        var component = CreateComponent<Ecs.Sources.Battle.Components.RoomIndexComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceRoomIndex(int newValue) {
        var index = BattleComponentsLookup.RoomIndex;
        var component = CreateComponent<Ecs.Sources.Battle.Components.RoomIndexComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveRoomIndex() {
        RemoveComponent(BattleComponentsLookup.RoomIndex);
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

    static Entitas.IMatcher<BattleEntity> _matcherRoomIndex;

    public static Entitas.IMatcher<BattleEntity> RoomIndex {
        get {
            if (_matcherRoomIndex == null) {
                var matcher = (Entitas.Matcher<BattleEntity>)Entitas.Matcher<BattleEntity>.AllOf(BattleComponentsLookup.RoomIndex);
                matcher.componentNames = BattleComponentsLookup.componentNames;
                _matcherRoomIndex = matcher;
            }

            return _matcherRoomIndex;
        }
    }
}
