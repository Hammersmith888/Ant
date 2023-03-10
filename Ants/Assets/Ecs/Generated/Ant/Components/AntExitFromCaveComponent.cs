//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    static readonly Ecs.Sources.Ant.Components.Flags.ExitFromCaveComponent exitFromCaveComponent = new Ecs.Sources.Ant.Components.Flags.ExitFromCaveComponent();

    public bool isExitFromCave {
        get { return HasComponent(AntComponentsLookup.ExitFromCave); }
        set {
            if (value != isExitFromCave) {
                var index = AntComponentsLookup.ExitFromCave;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : exitFromCaveComponent;

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

    static Entitas.IMatcher<AntEntity> _matcherExitFromCave;

    public static Entitas.IMatcher<AntEntity> ExitFromCave {
        get {
            if (_matcherExitFromCave == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.ExitFromCave);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherExitFromCave = matcher;
            }

            return _matcherExitFromCave;
        }
    }
}