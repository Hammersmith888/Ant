//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.AntTypeComponent antType { get { return (Ecs.Sources.Ant.Components.AntTypeComponent)GetComponent(AntComponentsLookup.AntType); } }
    public bool hasAntType { get { return HasComponent(AntComponentsLookup.AntType); } }

    public void AddAntType(AntType newValue) {
        var index = AntComponentsLookup.AntType;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AntTypeComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceAntType(AntType newValue) {
        var index = AntComponentsLookup.AntType;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AntTypeComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveAntType() {
        RemoveComponent(AntComponentsLookup.AntType);
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

    static Entitas.IMatcher<AntEntity> _matcherAntType;

    public static Entitas.IMatcher<AntEntity> AntType {
        get {
            if (_matcherAntType == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.AntType);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherAntType = matcher;
            }

            return _matcherAntType;
        }
    }
}
