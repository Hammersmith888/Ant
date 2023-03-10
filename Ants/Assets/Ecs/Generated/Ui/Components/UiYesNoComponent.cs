//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class UiEntity {

    public Ecs.Sources.Ui.Components.YesNoComponent yesNo { get { return (Ecs.Sources.Ui.Components.YesNoComponent)GetComponent(UiComponentsLookup.YesNo); } }
    public bool hasYesNo { get { return HasComponent(UiComponentsLookup.YesNo); } }

    public void AddYesNo(Ecs.Sources.Ui.Components.YesNoData newValue) {
        var index = UiComponentsLookup.YesNo;
        var component = CreateComponent<Ecs.Sources.Ui.Components.YesNoComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceYesNo(Ecs.Sources.Ui.Components.YesNoData newValue) {
        var index = UiComponentsLookup.YesNo;
        var component = CreateComponent<Ecs.Sources.Ui.Components.YesNoComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveYesNo() {
        RemoveComponent(UiComponentsLookup.YesNo);
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

    static Entitas.IMatcher<UiEntity> _matcherYesNo;

    public static Entitas.IMatcher<UiEntity> YesNo {
        get {
            if (_matcherYesNo == null) {
                var matcher = (Entitas.Matcher<UiEntity>)Entitas.Matcher<UiEntity>.AllOf(UiComponentsLookup.YesNo);
                matcher.componentNames = UiComponentsLookup.componentNames;
                _matcherYesNo = matcher;
            }

            return _matcherYesNo;
        }
    }
}
