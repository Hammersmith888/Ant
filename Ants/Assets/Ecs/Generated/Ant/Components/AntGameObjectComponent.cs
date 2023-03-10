//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class AntEntity {

    public Ecs.Sources.Ant.Components.GameObjectComponent gameObject { get { return (Ecs.Sources.Ant.Components.GameObjectComponent)GetComponent(AntComponentsLookup.GameObject); } }
    public bool hasGameObject { get { return HasComponent(AntComponentsLookup.GameObject); } }

    public void AddGameObject(UnityEngine.GameObject newValue) {
        var index = AntComponentsLookup.GameObject;
        var component = CreateComponent<Ecs.Sources.Ant.Components.GameObjectComponent>(index);
        component.Value = newValue;
        AddComponent(index, component);
    }

    public void ReplaceGameObject(UnityEngine.GameObject newValue) {
        var index = AntComponentsLookup.GameObject;
        var component = CreateComponent<Ecs.Sources.Ant.Components.GameObjectComponent>(index);
        component.Value = newValue;
        ReplaceComponent(index, component);
    }

    public void RemoveGameObject() {
        RemoveComponent(AntComponentsLookup.GameObject);
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

    static Entitas.IMatcher<AntEntity> _matcherGameObject;

    public static Entitas.IMatcher<AntEntity> GameObject {
        get {
            if (_matcherGameObject == null) {
                var matcher = (Entitas.Matcher<AntEntity>)Entitas.Matcher<AntEntity>.AllOf(AntComponentsLookup.GameObject);
                matcher.componentNames = AntComponentsLookup.componentNames;
                _matcherGameObject = matcher;
            }

            return _matcherGameObject;
        }
    }
}
