//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class UiEntity {

    static readonly Ecs.Sources.Ui.Components.Flags.InitComponent initComponent = new Ecs.Sources.Ui.Components.Flags.InitComponent();

    public bool isInit {
        get { return HasComponent(UiComponentsLookup.Init); }
        set {
            if (value != isInit) {
                var index = UiComponentsLookup.Init;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : initComponent;

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
public sealed partial class UiMatcher {

    static Entitas.IMatcher<UiEntity> _matcherInit;

    public static Entitas.IMatcher<UiEntity> Init {
        get {
            if (_matcherInit == null) {
                var matcher = (Entitas.Matcher<UiEntity>)Entitas.Matcher<UiEntity>.AllOf(UiComponentsLookup.Init);
                matcher.componentNames = UiComponentsLookup.componentNames;
                _matcherInit = matcher;
            }

            return _matcherInit;
        }
    }
}
