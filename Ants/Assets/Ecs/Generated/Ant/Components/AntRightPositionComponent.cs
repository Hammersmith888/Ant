//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.RightPositionComponent rightPositionComponent = new Ecs.Sources.Ant.Components.Flags.RightPositionComponent();

    public bool isRightPosition {
        get { return HasComponent(AntComponentsLookup.RightPosition); }
        set {
            if (value != isRightPosition) {
                var index = AntComponentsLookup.RightPosition;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : rightPositionComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherRightPosition;

    public static Entitas.IMatcher<AntEntity> RightPosition {
        get {
            if (_matcherRightPosition == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.RightPosition);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherRightPosition = matcher;
            }

            return _matcherRightPosition;
        }
    }
}