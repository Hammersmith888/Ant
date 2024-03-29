//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleContext {

    public BattleEntity moveCameraEntity { get { return GetGroup(BattleMatcher.MoveCamera).GetSingleEntity(); } }
    public Ecs.Sources.Battle.Components.MoveCameraComponent moveCamera { get { return moveCameraEntity.moveCamera; } }
    public bool hasMoveCamera { get { return moveCameraEntity != null; } }

    public BattleEntity SetMoveCamera(UnityEngine.Vector3 newFrom, UnityEngine.Vector3 newTo, float newStartTime) {
        if (hasMoveCamera) {
            throw new Entitas.EntitasException("Could not set MoveCamera!\n" + this + " already has an entity with Ecs.Sources.Battle.Components.MoveCameraComponent!",
                "You should check if the context already has a moveCameraEntity before setting it or use context.ReplaceMoveCamera().");
        }
        var entity = CreateEntity();
        entity.AddMoveCamera(newFrom, newTo, newStartTime);
        return entity;
    }

    public void ReplaceMoveCamera(UnityEngine.Vector3 newFrom, UnityEngine.Vector3 newTo, float newStartTime) {
        var entity = moveCameraEntity;
        if (entity == null) {
            entity = SetMoveCamera(newFrom, newTo, newStartTime);
        } else {
            entity.ReplaceMoveCamera(newFrom, newTo, newStartTime);
        }
    }

    public void RemoveMoveCamera() {
        moveCameraEntity.Destroy();
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleEntity {

    public Ecs.Sources.Battle.Components.MoveCameraComponent moveCamera { get { return (Ecs.Sources.Battle.Components.MoveCameraComponent)GetComponent(BattleComponentsLookup.MoveCamera); } }
    public bool hasMoveCamera { get { return HasComponent(BattleComponentsLookup.MoveCamera); } }

    public void AddMoveCamera(UnityEngine.Vector3 newFrom, UnityEngine.Vector3 newTo, float newStartTime) {
        var index = BattleComponentsLookup.MoveCamera;
        var component = CreateComponent<Ecs.Sources.Battle.Components.MoveCameraComponent>(index);
        component.From = newFrom;
        component.To = newTo;
        component.StartTime = newStartTime;
        AddComponent(index, component);
    }

    public void ReplaceMoveCamera(UnityEngine.Vector3 newFrom, UnityEngine.Vector3 newTo, float newStartTime) {
        var index = BattleComponentsLookup.MoveCamera;
        var component = CreateComponent<Ecs.Sources.Battle.Components.MoveCameraComponent>(index);
        component.From = newFrom;
        component.To = newTo;
        component.StartTime = newStartTime;
        ReplaceComponent(index, component);
    }

    public void RemoveMoveCamera() {
        RemoveComponent(BattleComponentsLookup.MoveCamera);
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
public sealed partial class BattleMatcher {

    static Entitas.IMatcher<BattleEntity> _matcherMoveCamera;

    public static Entitas.IMatcher<BattleEntity> MoveCamera {
        get {
            if (_matcherMoveCamera == null) {
                var matcher = (Entitas.Matcher<BattleEntity>)Entitas.Matcher<BattleEntity>.AllOf(BattleComponentsLookup.MoveCamera);
                matcher.componentNames = BattleComponentsLookup.componentNames;
                _matcherMoveCamera = matcher;
            }

            return _matcherMoveCamera;
        }
    }
}
