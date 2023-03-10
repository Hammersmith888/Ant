using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle
{
    public class FindAttackPositionSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly BattleService _battleService;

        public FindAttackPositionSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            BattleService battleService) : base(ant)
        {
            _ant = ant;
            _antSettings = antSettings;
            _battleService = battleService;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.FindAttackPosition);

        protected override bool Filter(AntEntity entity)
            => entity.isFindAttackPosition;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.isMoveToTarget = false;

                var targetEntity = _ant.GetEntityWithUid(entity.target.Uid);
                if (targetEntity.isDead || !targetEntity.hasAttackers)
                {
                    _battleService.SetNextTarget(entity);
                    return;
                }

                var attackers = targetEntity.attackers.Uids.ToList();
                //UnityEngine.Debug.Log($"[{nameof(FindAttackPositionSystem)}] {attackers.Count}");

                var target = entity.target.Transform;
                if (attackers.Count == 1)
                {
                    var attacker = _ant.GetEntityWithUid(attackers[0]);
                    var isLeft = attacker.transform.Value.position.x < target.position.x;

                    var destination = targetEntity.transform.Value.position;
                    if (isLeft)
                        destination.x += _antSettings.findAttackPositionOffset;
                    else
                        destination.x -= _antSettings.findAttackPositionOffset;

                    entity.AddMoveToPosition(destination);

                    attackers.Add(entity.uid.Value);
                    targetEntity.ReplaceAttackers(attackers.ToArray());
                }
                else if (attackers.Count == 2)
                {
                    var attacker = _ant.GetEntityWithUid(attackers[1]);
                    var isLeft = attacker.transform.Value.position.x < target.position.x;

                    var destination = targetEntity.transform.Value.position;
                    var offset = _antSettings.findAttackPositionOffset *
                                 _antSettings.findAttackPositionOffsetCoef;
                    if (isLeft)
                        destination.x += offset;
                    else
                        destination.x -= offset;

                    entity.AddMoveToPosition(destination);

                    attackers.Add(entity.uid.Value);
                    targetEntity.ReplaceAttackers(attackers.ToArray());
                }
                else
                {
                    //var attacker = _ant.GetEntityWithUid(attackers[2]);
                    //var isLeft = attacker.transform.Value.position.x < target.position.x;

                    var isLeft = false;
                    
                    var destination = targetEntity.transform.Value.position;
                    var offset = _antSettings.findAttackPositionOffset *
                                 _antSettings.findAttackPositionOffsetCoef;
                    if (isLeft)
                        destination.x += offset;
                    else
                        destination.x -= offset;

                    entity.AddMoveToPosition(destination);

                    attackers.Add(entity.uid.Value);
                    targetEntity.ReplaceAttackers(attackers.ToArray());
                }
            }
        }
    }
}