//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.DamageComponent damage { get { return (Ecs.Sources.Ant.Components.DamageComponent)GetComponent(AntComponentsLookup.Damage); } }
    public bool hasDamage { get { return HasComponent(AntComponentsLookup.Damage); } }

    public void AddDamage(float newValue) {
        var index = AntComponentsLookup.Damage;
        var component = CreateComponent<Ecs.Sources.Ant.Components.DamageComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceDamage(float newValue) {
        var index = AntComponentsLookup.Damage;
        var component = CreateComponent<Ecs.Sources.Ant.Components.DamageComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveDamage() {
        RemoveComponent(AntComponentsLookup.Damage);
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

    static Entitas.IMatcher<AntEntity> _matcherDamage;

    public static Entitas.IMatcher<AntEntity> Damage {
        get {
            if (_matcherDamage == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.Damage);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherDamage = matcher;
            }

            return _matcherDamage;
        }
    }
}
