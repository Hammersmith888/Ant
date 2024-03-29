//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleContext {

    public BattleEntity battleFinishEntity { get { return GetGroup(BattleMatcher.BattleFinish).GetSingleEntity(); } }

    public bool isBattleFinish {
        get { return battleFinishEntity != null; }
        set {
            var entity = battleFinishEntity;
            if (value != (entity != null)) {
                if (value) {
                    CreateEntity().isBattleFinish = true;
                } else {
                    entity.Destroy();
                }
            }
        }
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

    static readonly Ecs.Sources.Battle.Components.Flags.BattleFinishComponent battleFinishComponent = new Ecs.Sources.Battle.Components.Flags.BattleFinishComponent();

    public bool isBattleFinish {
        get { return HasComponent(BattleComponentsLookup.BattleFinish); }
        set {
            if (value != isBattleFinish) {
                var index = BattleComponentsLookup.BattleFinish;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : battleFinishComponent;

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
public sealed partial class BattleMatcher {

    static Entitas.IMatcher<BattleEntity> _matcherBattleFinish;

    public static Entitas.IMatcher<BattleEntity> BattleFinish {
        get {
            if (_matcherBattleFinish == null) {
                var matcher = (Entitas.Matcher<BattleEntity>)Entitas.Matcher<BattleEntity>.AllOf(BattleComponentsLookup.BattleFinish);
                matcher.componentNames = BattleComponentsLookup.componentNames;
                _matcherBattleFinish = matcher;
            }

            return _matcherBattleFinish;
        }
    }
}
