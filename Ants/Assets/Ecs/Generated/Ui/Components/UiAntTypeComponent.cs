//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class UiEntity {

    public Ecs.Sources.Ui.Components.AntTypeComponent antType { get { return (Ecs.Sources.Ui.Components.AntTypeComponent)GetComponent(UiComponentsLookup.AntType); } }
    public bool hasAntType { get { return HasComponent(UiComponentsLookup.AntType); } }

    public void AddAntType(AntType newValue) {
        var index = UiComponentsLookup.AntType;
        var component = CreateComponent<Ecs.Sources.Ui.Components.AntTypeComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceAntType(AntType newValue) {
        var index = UiComponentsLookup.AntType;
        var component = CreateComponent<Ecs.Sources.Ui.Components.AntTypeComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveAntType() {
        RemoveComponent(UiComponentsLookup.AntType);
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

    static Entitas.IMatcher<UiEntity> _matcherAntType;

    public static Entitas.IMatcher<UiEntity> AntType {
        get {
            if (_matcherAntType == null) {
                var matcher = (Entitas.Matcher<UiEntity>)Entitas.Matcher<UiEntity>.AllOf(UiComponentsLookup.AntType);
                matcher.componentNames = UiComponentsLookup.componentNames;
                _matcherAntType = matcher;
            }

            return _matcherAntType;
        }
    }
}