//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.SkeletonAnimationComponent skeletonAnimation { get { return (Ecs.Sources.Ant.Components.SkeletonAnimationComponent)GetComponent(AntComponentsLookup.SkeletonAnimation); } }
    public bool hasSkeletonAnimation { get { return HasComponent(AntComponentsLookup.SkeletonAnimation); } }

    public void AddSkeletonAnimation(Spine.Unity.SkeletonAnimation newValue) {
        var index = AntComponentsLookup.SkeletonAnimation;
        var component = CreateComponent<Ecs.Sources.Ant.Components.SkeletonAnimationComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceSkeletonAnimation(Spine.Unity.SkeletonAnimation newValue) {
        var index = AntComponentsLookup.SkeletonAnimation;
        var component = CreateComponent<Ecs.Sources.Ant.Components.SkeletonAnimationComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveSkeletonAnimation() {
        RemoveComponent(AntComponentsLookup.SkeletonAnimation);
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

    static Entitas.IMatcher<AntEntity> _matcherSkeletonAnimation;

    public static Entitas.IMatcher<AntEntity> SkeletonAnimation {
        get {
            if (_matcherSkeletonAnimation == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.SkeletonAnimation);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherSkeletonAnimation = matcher;
            }

            return _matcherSkeletonAnimation;
        }
    }
}
