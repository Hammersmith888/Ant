//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ContextMatcherGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class BattleMatcher {

    public static Entitas.IAllOfMatcher<BattleEntity> AllOf(params int[] indices) {
        return Entitas.Matcher<BattleEntity>.AllOf(indices);
    }

    public static Entitas.IAllOfMatcher<BattleEntity> AllOf(params Entitas.IMatcher<BattleEntity>[] matchers) {
          return Entitas.Matcher<BattleEntity>.AllOf(matchers);
    }

    public static Entitas.IAnyOfMatcher<BattleEntity> AnyOf(params int[] indices) {
          return Entitas.Matcher<BattleEntity>.AnyOf(indices);
    }

    public static Entitas.IAnyOfMatcher<BattleEntity> AnyOf(params Entitas.IMatcher<BattleEntity>[] matchers) {
          return Entitas.Matcher<BattleEntity>.AnyOf(matchers);
    }
}
