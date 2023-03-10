//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class UiEntity {

    static readonly Ecs.Sources.Ui.Components.Flags.UiComponent uiComponent = new Ecs.Sources.Ui.Components.Flags.UiComponent();

    public bool isUi {
        get { return HasComponent(UiComponentsLookup.Ui); }
        set {
            if (value != isUi) {
                var index = UiComponentsLookup.Ui;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : uiComponent;

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

    static Entitas.IMatcher<UiEntity> _matcherUi;

    public static Entitas.IMatcher<UiEntity> Ui {
        get {
            if (_matcherUi == null) {
                var matcher = (Entitas.Matcher<UiEntity>)Entitas.Matcher<UiEntity>.AllOf(UiComponentsLookup.Ui);
                matcher.componentNames = UiComponentsLookup.componentNames;
                _matcherUi = matcher;
            }

            return _matcherUi;
        }
    }
}
