//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.AttackTimeComponent attackTime { get { return (Ecs.Sources.Ant.Components.AttackTimeComponent)GetComponent(AntComponentsLookup.AttackTime); } }
    public bool hasAttackTime { get { return HasComponent(AntComponentsLookup.AttackTime); } }

    public void AddAttackTime(float newValue) {
        var index = AntComponentsLookup.AttackTime;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AttackTimeComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceAttackTime(float newValue) {
        var index = AntComponentsLookup.AttackTime;
        var component = CreateComponent<Ecs.Sources.Ant.Components.AttackTimeComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveAttackTime() {
        RemoveComponent(AntComponentsLookup.AttackTime);
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

    static Entitas.IMatcher<AntEntity> _matcherAttackTime;

    public static Entitas.IMatcher<AntEntity> AttackTime {
        get {
            if (_matcherAttackTime == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.AttackTime);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherAttackTime = matcher;
            }

            return _matcherAttackTime;
        }
    }
}