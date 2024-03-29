//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Custom.Components.NameComponent name { get { return (Ecs.Sources.Custom.Components.NameComponent)GetComponent(AntComponentsLookup.Name); } }
    public bool hasName { get { return HasComponent(AntComponentsLookup.Name); } }

    public void AddName(string newValue) {
        var index = AntComponentsLookup.Name;
        var component = CreateComponent<Ecs.Sources.Custom.Components.NameComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceName(string newValue) {
        var index = AntComponentsLookup.Name;
        var component = CreateComponent<Ecs.Sources.Custom.Components.NameComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveName() {
        RemoveComponent(AntComponentsLookup.Name);
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

    static Entitas.IMatcher<AntEntity> _matcherName;

    public static Entitas.IMatcher<AntEntity> Name {
        get {
            if (_matcherName == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.Name);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherName = matcher;
            }

            return _matcherName;
        }
    }
}
