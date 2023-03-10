//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.WaitOpponentComponent waitOpponentComponent = new Ecs.Sources.Ant.Components.Flags.WaitOpponentComponent();

    public bool isWaitOpponent {
        get { return HasComponent(AntComponentsLookup.WaitOpponent); }
        set {
            if (value != isWaitOpponent) {
                var index = AntComponentsLookup.WaitOpponent;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : waitOpponentComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherWaitOpponent;

    public static Entitas.IMatcher<AntEntity> WaitOpponent {
        get {
            if (_matcherWaitOpponent == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.WaitOpponent);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherWaitOpponent = matcher;
            }

            return _matcherWaitOpponent;
        }
    }
}
