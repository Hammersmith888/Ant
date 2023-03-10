using System.Collections.Generic;
using BugsFarm.Model.Enum;
using BugsFarm.Views.Fight;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class FightSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly FightView _fightView;

        public FightSystem(BattleContext battle,
            FightView fightView) : base(battle)
        {
            _battle = battle;
            _fightView = fightView;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.Fight;

        protected override void Execute(List<BattleEntity> entities)
        {
            _battle.isBattleFinish = false;
            
            var y = Camera.main.transform.position.y +
                    Camera.main.orthographicSize -
                    Tools.Size(_fightView.VeilTopSide).y / 2;
            _fightView.VeilTopSide.transform.SetY(y);

            MeleeGroups.Archers();
            Squads.Player.AllDo(Unit.mi_SelectTarget);
        }
    }
}