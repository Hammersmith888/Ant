//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.FadeInComponent fadeIn { get { return (Ecs.Sources.Ant.Components.FadeInComponent)GetComponent(AntComponentsLookup.FadeIn); } }
    public bool hasFadeIn { get { return HasComponent(AntComponentsLookup.FadeIn); } }

    public void AddFadeIn(float newTime) {
        var index = AntComponentsLookup.FadeIn;
        var component = CreateComponent<Ecs.Sources.Ant.Components.FadeInComponent>(index);
        component.Time = newTime;
        AddComponent(index, component);
    }

    public void ReplaceFadeIn(float newTime) {
        var index = AntComponentsLookup.FadeIn;
        var component = CreateComponent<Ecs.Sources.Ant.Components.FadeInComponent>(index);
        component.Time = newTime;
        ReplaceComponent(index, component);
    }

    public void RemoveFadeIn() {
        RemoveComponent(AntComponentsLookup.FadeIn);
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

    static Entitas.IMatcher<AntEntity> _matcherFadeIn;

    public static Entitas.IMatcher<AntEntity> FadeIn {
        get {
            if (_matcherFadeIn == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.FadeIn);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherFadeIn = matcher;
            }

            return _matcherFadeIn;
        }
    }
}
