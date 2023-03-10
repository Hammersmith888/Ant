//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.EnemyComponent enemyComponent = new Ecs.Sources.Ant.Components.Flags.EnemyComponent();

    public bool isEnemy {
        get { return HasComponent(AntComponentsLookup.Enemy); }
        set {
            if (value != isEnemy) {
                var index = AntComponentsLookup.Enemy;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : enemyComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherEnemy;

    public static Entitas.IMatcher<AntEntity> Enemy {
        get {
            if (_matcherEnemy == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.Enemy);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherEnemy = matcher;
            }

            return _matcherEnemy;
        }
    }
}