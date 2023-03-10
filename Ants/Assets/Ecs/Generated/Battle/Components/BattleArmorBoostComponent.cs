//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentContextApiGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial class BattleContext {

    public BattleEntity armorBoostEntity { get { return GetGroup(BattleMatcher.ArmorBoost).GetSingleEntity(); } }

    public bool isArmorBoost {
        get { return armorBoostEntity != null; }
        set {
            var entity = armorBoostEntity;
            if (value != (entity != null)) {
                if (value) {
                    CreateEntity().isArmorBoost = true;
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

    static readonly Ecs.Sources.Battle.Components.Flags.ArmorBoostComponent armorBoostComponent = new Ecs.Sources.Battle.Components.Flags.ArmorBoostComponent();

    public bool isArmorBoost {
        get { return HasComponent(BattleComponentsLookup.ArmorBoost); }
        set {
            if (value != isArmorBoost) {
                var index = BattleComponentsLookup.ArmorBoost;
                if (value) {
                    var componentPool = GetComponentPool(index);
                    var component = componentPool.Count > 0
                            ? componentPool.Pop()
                            : armorBoostComponent;

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

    static Entitas.IMatcher<BattleEntity> _matcherArmorBoost;

    public static Entitas.IMatcher<BattleEntity> ArmorBoost {
        get {
            if (_matcherArmorBoost == null) {
                var matcher = (Entitas.Matcher<BattleEntity>)Entitas.Matcher<BattleEntity>.AllOf(BattleComponentsLookup.ArmorBoost);
                matcher.componentNames = BattleComponentsLookup.componentNames;
                _matcherArmorBoost = matcher;
            }

            return _matcherArmorBoost;
        }
    }
}
