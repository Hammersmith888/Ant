using System.Collections.Generic;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using BugsFarm.SimulationSystem;
using BugsFarm.SimulationSystem.Obsolete;
using DG.Tweening;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class NoneStateSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly BattleService _battleService;

        public NoneStateSystem(BattleContext battle,
            BattleService battleService) : base(battle)
        {
            _battle = battle;
            _battleService = battleService;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.None;

        protected override void Execute(List<BattleEntity> entities)
        {
            if (!_battle.isInit)
            {
                _battle.isInit = true;
                return;
            }
            
            Camera.main.transform.DOKill();
            
            MeleeGroups.Clear();
            Arrows.Clear();
            SimulationOld.Instance.SimulateFrom(_battle.squadSelectOpenTime.Value);
            Sounds.Instance.SetReverb();
            Refs.Instance.MusicBattle.Stop();
            Refs.Instance.MusicFarm.FadeMute(false);
            Camera.main.transform.SetY(0);
            _battleService.SetGround(); // AFTER Camera.main.transform.SetY()
        }
    }
}