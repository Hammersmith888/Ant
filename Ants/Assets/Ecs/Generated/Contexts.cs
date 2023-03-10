//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ContextsGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class Contexts : Entitas.IContexts {

    public static Contexts sharedInstance {
        get {
            if (_sharedInstance == null) {
                _sharedInstance = new Contexts();
            }

            return _sharedInstance;
        }
        set { _sharedInstance = value; }
    }

    static Contexts _sharedInstance;

    public AntContext ant { get; set; }
    public BattleContext battle { get; set; }
    public GameContext game { get; set; }
    public InputContext input { get; set; }
    public ResourceContext resource { get; set; }
    public UiContext ui { get; set; }

    public Entitas.IContext[] allContexts { get { return new Entitas.IContext [] { ant, battle, game, input, resource, ui }; } }

    public Contexts() {
        ant = new AntContext();
        battle = new BattleContext();
        game = new GameContext();
        input = new InputContext();
        resource = new ResourceContext();
        ui = new UiContext();

        var postConstructors = System.Linq.Enumerable.Where(
            GetType().GetMethods(),
            method => System.Attribute.IsDefined(method, typeof(Entitas.CodeGeneration.Attributes.PostConstructorAttribute))
        );

        foreach (var postConstructor in postConstructors) {
            postConstructor.Invoke(this, null);
        }
    }

    public void Reset() {
        var contexts = allContexts;
        for (int i = 0; i < contexts.Length; i++) {
            contexts[i].Reset();
        }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.EntityIndexGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class Contexts {

    public const string ChestRoomIndex = "ChestRoomIndex";
    public const string Id = "Id";
    public const string RoomIndex = "RoomIndex";
    public const string Uid = "Uid";

    [Entitas.CodeGeneration.Attributes.PostConstructor]
    public void InitializeEntityIndices() {
        ant.AddEntityIndex(new Entitas.PrimaryEntityIndex<AntEntity, int>(
            ChestRoomIndex,
            ant.GetGroup(AntMatcher.ChestRoomIndex),
            (e, c) => ((Ecs.Sources.Ant.Components.ChestRoomIndexComponent)c).Value));

        ant.AddEntityIndex(new Entitas.PrimaryEntityIndex<AntEntity, int>(
            Id,
            ant.GetGroup(AntMatcher.Id),
            (e, c) => ((Ecs.Sources.Custom.Components.IdComponent)c).Value));

        battle.AddEntityIndex(new Entitas.PrimaryEntityIndex<BattleEntity, int>(
            RoomIndex,
            battle.GetGroup(BattleMatcher.RoomIndex),
            (e, c) => ((Ecs.Sources.Battle.Components.RoomIndexComponent)c).Value));

        ant.AddEntityIndex(new Entitas.PrimaryEntityIndex<AntEntity, Ecs.Utils.Uid>(
            Uid,
            ant.GetGroup(AntMatcher.Uid),
            (e, c) => ((Ecs.Sources.Custom.Components.UidComponent)c).Value));
        battle.AddEntityIndex(new Entitas.PrimaryEntityIndex<BattleEntity, Ecs.Utils.Uid>(
            Uid,
            battle.GetGroup(BattleMatcher.Uid),
            (e, c) => ((Ecs.Sources.Custom.Components.UidComponent)c).Value));
    }
}

public static class ContextsExtensions {

    public static AntEntity GetEntityWithChestRoomIndex(this AntContext context, int Value) {
        return ((Entitas.PrimaryEntityIndex<AntEntity, int>)context.GetEntityIndex(Contexts.ChestRoomIndex)).GetEntity(Value);
    }

    public static AntEntity GetEntityWithId(this AntContext context, int Value) {
        return ((Entitas.PrimaryEntityIndex<AntEntity, int>)context.GetEntityIndex(Contexts.Id)).GetEntity(Value);
    }

    public static BattleEntity GetEntityWithRoomIndex(this BattleContext context, int Value) {
        return ((Entitas.PrimaryEntityIndex<BattleEntity, int>)context.GetEntityIndex(Contexts.RoomIndex)).GetEntity(Value);
    }

    public static AntEntity GetEntityWithUid(this AntContext context, Ecs.Utils.Uid Value) {
        return ((Entitas.PrimaryEntityIndex<AntEntity, Ecs.Utils.Uid>)context.GetEntityIndex(Contexts.Uid)).GetEntity(Value);
    }

    public static BattleEntity GetEntityWithUid(this BattleContext context, Ecs.Utils.Uid Value) {
        return ((Entitas.PrimaryEntityIndex<BattleEntity, Ecs.Utils.Uid>)context.GetEntityIndex(Contexts.Uid)).GetEntity(Value);
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.VisualDebugging.CodeGeneration.Plugins.ContextObserverGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class Contexts {

#if (!ENTITAS_DISABLE_VISUAL_DEBUGGING && UNITY_EDITOR)

    [Entitas.CodeGeneration.Attributes.PostConstructor]
    public void InitializeContexObservers() {
        try {
            CreateContextObserver(ant);
            CreateContextObserver(battle);
            CreateContextObserver(game);
            CreateContextObserver(input);
            CreateContextObserver(resource);
            CreateContextObserver(ui);
        } catch(System.Exception) {
        }
    }

    public void CreateContextObserver(Entitas.IContext context) {
        if (UnityEngine.Application.isPlaying) {
            var observer = new Entitas.VisualDebugging.Unity.ContextObserver(context);
            UnityEngine.Object.DontDestroyOnLoad(observer.gameObject);
        }
    }

#endif
}