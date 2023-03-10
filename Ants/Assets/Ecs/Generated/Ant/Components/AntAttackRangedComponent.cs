//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.AttackRangedComponent attackRangedComponent = new Ecs.Sources.Ant.Components.Flags.AttackRangedComponent();

    public bool isAttackRanged {
        get { return HasComponent(AntComponentsLookup.AttackRanged); }
        set {
            if (value != isAttackRanged) {
                var index = AntComponentsLookup.AttackRanged;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : attackRangedComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherAttackRanged;

    public static Entitas.IMatcher<AntEntity> AttackRanged {
        get {
            if (_matcherAttackRanged == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.AttackRanged);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherAttackRanged = matcher;
            }

            return _matcherAttackRanged;
        }
    }
}
