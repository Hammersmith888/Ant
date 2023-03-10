//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.FadeOutComponent fadeOut { get { return (Ecs.Sources.Ant.Components.FadeOutComponent)GetComponent(AntComponentsLookup.FadeOut); } }
    public bool hasFadeOut { get { return HasComponent(AntComponentsLookup.FadeOut); } }

    public void AddFadeOut(float newTime) {
        var index = AntComponentsLookup.FadeOut;
        var component = CreateComponent<Ecs.Sources.Ant.Components.FadeOutComponent>(index);
        component.Time = newTime;
        AddComponent(index, component);
    }

    public void ReplaceFadeOut(float newTime) {
        var index = AntComponentsLookup.FadeOut;
        var component = CreateComponent<Ecs.Sources.Ant.Components.FadeOutComponent>(index);
        component.Time = newTime;
        ReplaceComponent(index, component);
    }

    public void RemoveFadeOut() {
        RemoveComponent(AntComponentsLookup.FadeOut);
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

    static Entitas.IMatcher<AntEntity> _matcherFadeOut;

    public static Entitas.IMatcher<AntEntity> FadeOut {
        get {
            if (_matcherFadeOut == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.FadeOut);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherFadeOut = matcher;
            }

            return _matcherFadeOut;
        }
    }
}