//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.DeadComponent deadComponent = new Ecs.Sources.Ant.Components.Flags.DeadComponent();

    public bool isDead {
        get { return HasComponent(AntComponentsLookup.Dead); }
        set {
            if (value != isDead) {
                var index = AntComponentsLookup.Dead;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : deadComponent;

                    AddComponent(index, component);
                } else {
                    RemoveComponent(index);
                }
            }
        }
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

    static Entitas.IMatcher<AntEntity> _matcherDead;

    public static Entitas.IMatcher<AntEntity> Dead {
        get {
            if (_matcherDead == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.Dead);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherDead = matcher;
            }

            return _matcherDead;
        }
    }
}
