//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.PatrolComponent patrolComponent = new Ecs.Sources.Ant.Components.Flags.PatrolComponent();

    public bool isPatrol {
        get { return HasComponent(AntComponentsLookup.Patrol); }
        set {
            if (value != isPatrol) {
                var index = AntComponentsLookup.Patrol;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : patrolComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherPatrol;

    public static Entitas.IMatcher<AntEntity> Patrol {
        get {
            if (_matcherPatrol == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.Patrol);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherPatrol = matcher;
            }

            return _matcherPatrol;
        }
    }
}
