//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.ChestRoomIndexComponent chestRoomIndex { get { return (Ecs.Sources.Ant.Components.ChestRoomIndexComponent)GetComponent(AntComponentsLookup.ChestRoomIndex); } }
    public bool hasChestRoomIndex { get { return HasComponent(AntComponentsLookup.ChestRoomIndex); } }

    public void AddChestRoomIndex(int newValue) {
        var index = AntComponentsLookup.ChestRoomIndex;
        var component = CreateComponent<Ecs.Sources.Ant.Components.ChestRoomIndexComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceChestRoomIndex(int newValue) {
        var index = AntComponentsLookup.ChestRoomIndex;
        var component = CreateComponent<Ecs.Sources.Ant.Components.ChestRoomIndexComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveChestRoomIndex() {
        RemoveComponent(AntComponentsLookup.ChestRoomIndex);
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
public sealed partial class AntMatcher {

    static Entitas.IMatcher<AntEntity> _matcherChestRoomIndex;

    public static Entitas.IMatcher<AntEntity> ChestRoomIndex {
        get {
            if (_matcherChestRoomIndex == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.ChestRoomIndex);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherChestRoomIndex = matcher;
            }

            return _matcherChestRoomIndex;
        }
    }
}
