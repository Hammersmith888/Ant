using System.Collections.Generic;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using BugsFarm.Views.Fight;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class SpawnSystem : ReactiveSystem<BattleEntity>
    {
        private readonly AntService _antService;
        private readonly AntContext _ant;
        private readonly BattleContext _battle;
        private readonly BattleService _battleService;
        private readonly FightView _fightView;

        public SpawnSystem(
            AntService antService,
            AntContext ant,
            BattleContext battle,
            BattleService battleService,
            FightView fightView) : base(battle)
        {
            _antService = antService;
            _ant = ant;
            _battle = battle;
            _battleService = battleService;
            _fightView = fightView;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);


        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.Spawn;

        protected override void Execute(List<BattleEntity> entities)
        {
            var y = Camera.main.transform.position.y +
                    Camera.main.orthographicSize -
                    Tools.Size(_fightView.VeilTopSide).y / 2;
            _fightView.VeilTopSide.transform.SetY(y);

            _battleService.SetGround();
            Squads.Spawn(_antService);
            Sounds.Instance.SetReverb();
            Refs.Instance.MusicFarm.FadeMute(true);
            Refs.Instance.MusicBattle.Play();
            //_battle.ReplaceFightState(EFightState.TurnLightOn);
        }
    }
}