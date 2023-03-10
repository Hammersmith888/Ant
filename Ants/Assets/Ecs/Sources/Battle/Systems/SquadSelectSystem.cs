using System.Collections.Generic;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using BugsFarm.Views.Fight;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class SquadSelectSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly BattleService _battleService;
        private readonly FightView _fightView;

        public SquadSelectSystem(BattleContext battle,
            BattleService battleService,
            FightView fightView) : base(battle)
        {
            _battle = battle;
            _battleService = battleService;
            _fightView = fightView;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.SquadSelect;

        protected override void Execute(List<BattleEntity> entities)
        {
            var y = Camera.main.transform.position.y +
                    Camera.main.orthographicSize -
                    Tools.Size(_fightView.VeilTopSide).y / 2;
            _fightView.VeilTopSide.transform.SetY(y);

            _battle.ReplaceSquadSelectOpenTime(Tools.UtcNow());
            _battleService.SetGround();
        }
    }
}