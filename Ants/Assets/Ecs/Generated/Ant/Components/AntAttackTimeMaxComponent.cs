//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.AttackTimeMaxComponent attackTimeMax { get { return (Ecs.Sources.Ant.Components.AttackTimeMaxComponent)GetComponent(AntComponentsLookup.AttackTimeMax); } }
    public bool hasAttackTimeMax { get { return HasComponent(AntComponentsLookup.AttackTimeMax); } }

    public void AddAttackTimeMax(float newValue) {
        var index = AntComponentsLookup.AttackTimeMax;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AttackTimeMaxComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceAttackTimeMax(float newValue) {
        var index = AntComponentsLookup.AttackTimeMax;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AttackTimeMaxComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveAttackTimeMax() {
        RemoveComponent(AntComponentsLookup.AttackTimeMax);
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

    static Entitas.IMatcher<AntEntity> _matcherAttackTimeMax;

    public static Entitas.IMatcher<AntEntity> AttackTimeMax {
        get {
            if (_matcherAttackTimeMax == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.AttackTimeMax);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherAttackTimeMax = matcher;
            }

            return _matcherAttackTimeMax;
        }
    }
}
